using IceSync.Domain.Entities;

namespace IceSync.Domain.Contracts.Managers;

public interface IWorkflowManager
{
    Task SynchronizeWorkflowsAsync();
    Task<IEnumerable<Workflow>> GetWorkflowsAsync();
    Task<bool> RunWorkflowAsync(int workflowId);
}
