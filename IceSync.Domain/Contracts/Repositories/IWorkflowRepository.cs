using IceSync.Domain.Entities;

namespace IceSync.Domain.Contracts;

public interface IWorkflowRepository
{
    Task<IEnumerable<Workflow>> GetWorkflowsAsync();
    Task AddWorkflowsAsync(IEnumerable<Workflow> workflows);
    Task UpdateWorkflowsAsync(IEnumerable<Workflow> workflows);
    Task DeleteWorkflowsAsync(IEnumerable<int> workflowIds);
}
