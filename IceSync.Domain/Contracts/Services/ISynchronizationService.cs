namespace IceSync.Domain.Contracts.Services;

public interface ISynchronizationService
{
    Task SynchronizeWorkflowsAsync();
}