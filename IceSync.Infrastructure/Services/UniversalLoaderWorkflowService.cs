﻿using IceSync.Domain.Contracts;
using IceSync.Infrastructure.ApiClients;
using IceSync.Infrastructure.Mappers;
using Microsoft.Extensions.Logging;

namespace IceSync.Infrastructure.Services;

public class UniversalLoaderWorkflowService : IExternalWorkflowService
{
    private readonly UniversalLoaderAPIClient _apiClient;
    private readonly ILogger<UniversalLoaderWorkflowService> _logger;

    public UniversalLoaderWorkflowService(UniversalLoaderAPIClient apiClient, ILogger<UniversalLoaderWorkflowService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IEnumerable<Domain.Entities.Workflow>> GetWorkflowsAsync()
    {
        _logger.LogInformation("Fetching workflows from UniversalLoader API.");

        try
        {
            var workflows = await _apiClient.WorkflowsAsync();
            var result = workflows.Select(w => WorkFlowMapper.MapToDomainWorkflow(w));

            _logger.LogInformation("Successfully fetched workflows.");

            return result;
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, "Error occurred while fetching workflows from UniversalLoader API.");
            throw;
        }
    }

    public async Task<bool> RunWorkflowAsync(int workflowId)
    {
        _logger.LogInformation($"Running workflow with ID {workflowId}.");

        try
        {
            await _apiClient.WorkflowsRunAsync(workflowId, null, null);
            _logger.LogInformation($"Successfully triggered workflow with ID {workflowId}.");
            return true;
        }
        catch(ApiException ex)
        {
            _logger.LogError(ex, $"Error occurred while running workflow with ID {workflowId}.");
            return false;
        }
    }
}
