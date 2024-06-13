using IceSync.Domain.Entities;

namespace IceSync.Domain.Contracts;

public interface IExternalWorkflowService
{
    Task<IEnumerable<Workflow>> GetWorkflowsAsync();
    Task<bool> RunWorkflowAsync(int workflowId);
}
