using IceSync.Domain.Contracts;
using IceSync.Domain.Contracts.Managers;
using IceSync.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IceSync.Application.Managers;

public class WorkflowManager : IWorkflowManager
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IExternalWorkflowService _externalWorkflowService;
    private readonly ILogger<WorkflowManager> _logger;

    public WorkflowManager(
               IWorkflowRepository workflowRepository,
               IExternalWorkflowService externalWorkflowService,
               ILogger<WorkflowManager> logger)
    {
        _workflowRepository = workflowRepository;
        _externalWorkflowService = externalWorkflowService;
        _logger = logger;
    }

    public async Task<IEnumerable<Workflow>> GetWorkflowsAsync()
    {
        return await _externalWorkflowService.GetWorkflowsAsync();
    }

    public async Task<bool> RunWorkflowAsync(int workflowId)
    {
        var success = await _externalWorkflowService.RunWorkflowAsync(workflowId);
        return success;
    }

    public async Task SynchronizeWorkflowsAsync()
    {
        _logger.LogInformation("Starting workflow synchronization.");

        try
        {
            var (apiWorkflows, dbWorkflows) = await RetrieveWorkflowsAsync();

            if(!apiWorkflows.Any())
            {
                _logger.LogWarning("No workflows found in the external service.");
                return;
            }

            var workflowsToAdd = GetWorkflowsToAdd(apiWorkflows, dbWorkflows);
            var workflowsToDelete = GetWorkflowsToDelete(apiWorkflows, dbWorkflows);
            var workflowsToUpdate = GetWorkflowsToUpdate(apiWorkflows, dbWorkflows);

            await SynchronizeWorkflowsAsync(workflowsToAdd, workflowsToDelete, workflowsToUpdate);

            _logger.LogInformation("Workflow synchronization completed successfully.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred during workflow synchronization.");
        }
    }

    private async Task<(Dictionary<int, Workflow> ApiWorkflows, Dictionary<int, Workflow> DbWorkflows)> RetrieveWorkflowsAsync()
    {
        var apiWorkflowsTask = _externalWorkflowService.GetWorkflowsAsync();
        var dbWorkflowsTask = _workflowRepository.GetWorkflowsAsync();

        await Task.WhenAll(apiWorkflowsTask, dbWorkflowsTask);

        var apiResult = apiWorkflowsTask.Result ?? Enumerable.Empty<Workflow>();
        var dbResult = dbWorkflowsTask.Result ?? Enumerable.Empty<Workflow>();

        return (apiResult.ToDictionary(w => w.WorkflowID), dbResult.ToDictionary(d => d.WorkflowID));
    }

    private List<Workflow> GetWorkflowsToAdd(Dictionary<int, Workflow> apiWorkflows, Dictionary<int, Workflow> dbWorkflows)
    {
        return apiWorkflows
            .Where(x => !dbWorkflows.ContainsKey(x.Key))
            .Select(y => y.Value)
            .ToList();
    }

    private List<Workflow> GetWorkflowsToDelete(Dictionary<int, Workflow> apiWorkflows, Dictionary<int, Workflow> dbWorkflows)
    {
        return dbWorkflows
            .Where(x => !apiWorkflows.ContainsKey(x.Key))
            .Select(y => y.Value)
            .ToList();
    }

    private List<Workflow> GetWorkflowsToUpdate(Dictionary<int, Workflow> apiWorkflows, Dictionary<int, Workflow> dbWorkflows)
    {
        return apiWorkflows
            .Where(x => dbWorkflows.ContainsKey(x.Key))
            .Select(y => y.Value)
            .ToList();
    }

    private async Task SynchronizeWorkflowsAsync(List<Workflow> workflowsToAdd, List<Workflow> workflowsToDelete, List<Workflow> workflowsToUpdate)
    {
        if(workflowsToAdd.Any())
        {
            await _workflowRepository.AddWorkflowsAsync(workflowsToAdd);
        }

        if(workflowsToDelete.Any())
        {
            List<int> workflowIdsToDelete = workflowsToDelete.Select(w => w.WorkflowID).ToList();
            await _workflowRepository.DeleteWorkflowsAsync(workflowIdsToDelete);
        }

        if(workflowsToUpdate.Any())
        {
            await _workflowRepository.UpdateWorkflowsAsync(workflowsToUpdate);
        }
    }
}
