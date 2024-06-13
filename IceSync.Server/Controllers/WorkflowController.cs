using IceSync.Domain.Contracts.Managers;
using Microsoft.AspNetCore.Mvc;

namespace IceSync.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowManager _workflowManager;

    public WorkflowController(IWorkflowManager workflowManager)
    {
        _workflowManager = workflowManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWorkflows()
    {
        try
        {
            var workflows = await _workflowManager.GetWorkflowsAsync();
            return Ok(workflows);
        }
        catch(Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred while retrieving workflows." });
        }
    }

    [HttpPost("{workflowId}/run")]
    public async Task<IActionResult> RunWorkflow(int workflowId)
    {
        bool success = await _workflowManager.RunWorkflowAsync(workflowId);

        return Ok(new { success, Message = success ? "Workflow executed successfully" : "Failed to execute workflow" });
    }
}
