namespace IceSync.Domain.Entities;

public class Workflow
{
    public int WorkflowID { get; set; }
    public string? WorkflowName { get; set; }
    public bool IsActive { get; set; }
    public string? MultiExecBehavior { get; set; }
}