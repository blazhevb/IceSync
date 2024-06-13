using IceSync.Infrastructure.Contracts;

namespace IceSync.Infrastructure.Mappers;

public class WorkFlowMapper : IWorkflowMapper
{
    public Domain.Entities.Workflow MapToDomainWorkflow(ApiClients.Workflow source)
    {
        Domain.Entities.Workflow target = new Domain.Entities.Workflow();

        target.WorkflowID = source.Id;
        target.WorkflowName = source.Name;
        target.IsActive = source.IsActive;
        target.MultiExecBehavior = source.MultiExecBehavior;   
        
        return target;
    }
}
