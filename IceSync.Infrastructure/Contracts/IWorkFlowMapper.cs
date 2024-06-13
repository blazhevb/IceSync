using IceSync.Infrastructure.ApiClients;

namespace IceSync.Infrastructure.Contracts;

public interface IWorkflowMapper
{
    Domain.Entities.Workflow MapToDomainWorkflow(Workflow workflow);
}
