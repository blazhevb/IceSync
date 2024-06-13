using IceSync.Infrastructure.Mappers;

namespace IceSync.Tests.Infrastructure.Tests;

[TestClass]
public class WorkFlowMapperTests
{
    [TestMethod]
    public void MapToDomainWorkflow_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var apiWorkflow = new IceSync.Infrastructure.ApiClients.Workflow
        {
            Id = 1,
            Name = "Test Workflow",
            IsActive = true,
            MultiExecBehavior = "Test Behavior"
        };

        // Act
        var result = WorkFlowMapper.MapToDomainWorkflow(apiWorkflow);

        // Assert
        Assert.AreEqual(apiWorkflow.Id, result.WorkflowID);
        Assert.AreEqual(apiWorkflow.Name, result.WorkflowName);
        Assert.AreEqual(apiWorkflow.IsActive, result.IsActive);
        Assert.AreEqual(apiWorkflow.MultiExecBehavior, result.MultiExecBehavior);
    }

    [TestMethod]
    public void MapToDomainWorkflow_ShouldHandleNullValues()
    {
        // Arrange
        var apiWorkflow = new IceSync.Infrastructure.ApiClients.Workflow
        {
            Id = 1,
            Name = null,
            IsActive = true,
            MultiExecBehavior = null
        };

        // Act
        var result = WorkFlowMapper.MapToDomainWorkflow(apiWorkflow);

        // Assert
        Assert.AreEqual(apiWorkflow.Id, result.WorkflowID);
        Assert.IsNull(result.WorkflowName);
        Assert.AreEqual(apiWorkflow.IsActive, result.IsActive);
        Assert.IsNull(result.MultiExecBehavior);
    }

    [TestMethod]
    public void MapToDomainWorkflow_ShouldHandleFalseIsActive()
    {
        // Arrange
        var apiWorkflow = new IceSync.Infrastructure.ApiClients.Workflow
        {
            Id = 2,
            Name = "Inactive Workflow",
            IsActive = false,
            MultiExecBehavior = "Some Behavior"
        };

        // Act
        var result = WorkFlowMapper.MapToDomainWorkflow(apiWorkflow);

        // Assert
        Assert.AreEqual(apiWorkflow.Id, result.WorkflowID);
        Assert.AreEqual(apiWorkflow.Name, result.WorkflowName);
        Assert.AreEqual(apiWorkflow.IsActive, result.IsActive);
        Assert.AreEqual(apiWorkflow.MultiExecBehavior, result.MultiExecBehavior);
    }

    [TestMethod]
    public void MapToDomainWorkflow_ShouldHandleEmptyStrings()
    {
        // Arrange
        var apiWorkflow = new IceSync.Infrastructure.ApiClients.Workflow
        {
            Id = 3,
            Name = string.Empty,
            IsActive = true,
            MultiExecBehavior = string.Empty
        };

        // Act
        var result = WorkFlowMapper.MapToDomainWorkflow(apiWorkflow);

        // Assert
        Assert.AreEqual(apiWorkflow.Id, result.WorkflowID);
        Assert.AreEqual(apiWorkflow.Name, result.WorkflowName);
        Assert.AreEqual(apiWorkflow.IsActive, result.IsActive);
        Assert.AreEqual(apiWorkflow.MultiExecBehavior, result.MultiExecBehavior);
    }
}
