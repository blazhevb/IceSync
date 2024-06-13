using IceSync.Domain.Contracts.Managers;
using IceSync.Domain.Entities;
using IceSync.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IceSync.Tests.Server.Tests;

[TestClass]
public class WorkflowControllerTests
{
    private Mock<IWorkflowManager> _workflowManagerMock;
    private WorkflowController _sut;

    [TestInitialize]
    public void Setup()
    {
        _workflowManagerMock = new Mock<IWorkflowManager>();
        _sut = new WorkflowController(_workflowManagerMock.Object);
    }

    [TestMethod]
    public async Task GetAllWorkflows_ShouldReturnOkResult_WithListOfWorkflows()
    {
        // Arrange
        var workflows = new List<Workflow>
        {
            new Workflow { WorkflowID = 1, WorkflowName = "Workflow 1", IsActive = true },
            new Workflow { WorkflowID = 2, WorkflowName = "Workflow 2", IsActive = false }
        };
        _workflowManagerMock.Setup(m => m.GetWorkflowsAsync()).ReturnsAsync(workflows);

        // Act
        var result = await _sut.GetAllWorkflows() as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(workflows, result.Value);
    }

    [TestMethod]
    public async Task GetAllWorkflows_ShouldReturnInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        _workflowManagerMock.Setup(m => m.GetWorkflowsAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _sut.GetAllWorkflows() as ObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);     
    }


    [TestMethod]
    public async Task RunWorkflow_ShouldReturnOkResult_WhenWorkflowRunsSuccessfully()
    {
        // Arrange
        var workflowId = 1;
        _workflowManagerMock.Setup(m => m.RunWorkflowAsync(workflowId)).ReturnsAsync(true);

        // Act
        var result = await _sut.RunWorkflow(workflowId) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);        
    }

    [TestMethod]
    public async Task RunWorkflow_ShouldReturnOkResult_WhenWorkflowFails()
    {
        // Arrange
        var workflowId = 1;
        _workflowManagerMock.Setup(m => m.RunWorkflowAsync(workflowId)).ReturnsAsync(false);

        // Act
        var result = await _sut.RunWorkflow(workflowId) as OkObjectResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
    }
}
