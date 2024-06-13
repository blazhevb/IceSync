using IceSync.Application.Managers;
using IceSync.Domain.Contracts;
using IceSync.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IceSync.Tests.Application.Tests
{
    [TestClass]
    public class WorkflowManagerTests
    {
        private readonly Mock<IWorkflowRepository> _workflowRepositoryMock;
        private readonly Mock<IExternalWorkflowService> _externalWorkflowServiceMock;
        private readonly Mock<ILogger<WorkflowManager>> _loggerMock;
        private readonly WorkflowManager _sut;

        public WorkflowManagerTests()
        {
            _workflowRepositoryMock = new Mock<IWorkflowRepository>();
            _externalWorkflowServiceMock = new Mock<IExternalWorkflowService>();
            _loggerMock = new Mock<ILogger<WorkflowManager>>();
            _sut = new WorkflowManager(_workflowRepositoryMock.Object, _externalWorkflowServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task GetWorkflowsAsync_ShouldReturnWorkflows_FromExternalService()
        {
            // Arrange
            var expectedWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 1, WorkflowName = "Test Workflow 1", IsActive = true },
                new Workflow { WorkflowID = 2, WorkflowName = "Test Workflow 2", IsActive = true }
            };

            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(expectedWorkflows);

            // Act
            var result = await _sut.GetWorkflowsAsync();

            // Assert
            Assert.AreEqual(expectedWorkflows.Count, result.Count());
            Assert.IsTrue(result.SequenceEqual(expectedWorkflows));
        }

        [TestMethod]
        public async Task RunWorkflowAsync_ShouldReturnTrue_WhenWorkflowRunsSuccessfully()
        {
            // Arrange
            var workflowId = 1;
            _externalWorkflowServiceMock.Setup(service => service.RunWorkflowAsync(workflowId))
                .ReturnsAsync(true);

            // Act
            var result = await _sut.RunWorkflowAsync(workflowId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RunWorkflowAsync_ShouldReturnFalse_WhenWorkflowFails()
        {
            // Arrange
            var workflowId = 1;
            _externalWorkflowServiceMock.Setup(service => service.RunWorkflowAsync(workflowId))
                .ReturnsAsync(false);

            // Act
            var result = await _sut.RunWorkflowAsync(workflowId);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldLogInformation_WhenSynchronizationStartsAndCompletes()
        {
            // Arrange
            var apiWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 1, WorkflowName = "API Workflow 1", IsActive = true }
            };
            var dbWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 2, WorkflowName = "DB Workflow 2", IsActive = true }
            };

            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowsAsync())
                .ReturnsAsync(dbWorkflows);

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldLogWarning_WhenNoWorkflowsFoundInExternalService()
        {
            // Arrange
            var apiWorkflows = new List<Workflow>();

            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
       
        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldAddUpdateAndDeleteWorkflowsAsExpected()
        {
            // Arrange
            var apiWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 1, WorkflowName = "API Workflow 1", IsActive = true },
                new Workflow { WorkflowID = 3, WorkflowName = "API Workflow 3", IsActive = true },
                new Workflow { WorkflowID = 2, WorkflowName = "DB Workflow 2 Updated", IsActive = true } // Assuming it's updated in API
            };
            var dbWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 2, WorkflowName = "DB Workflow 2", IsActive = true }
            };
        
            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowsAsync())
                .ReturnsAsync(dbWorkflows);
        
            // Act
            await _sut.SynchronizeWorkflowsAsync();
        
            // Assert
            _workflowRepositoryMock.Verify(repo => repo.AddWorkflowsAsync(It.Is<IEnumerable<Workflow>>(w => w.Count() == 2 && w.Any(x => x.WorkflowID == 1) && w.Any(x => x.WorkflowID == 3))), Times.Once);
            _workflowRepositoryMock.Verify(repo => repo.UpdateWorkflowsAsync(It.Is<IEnumerable<Workflow>>(w => w.Count() == 1 && w.Any(x => x.WorkflowID == 2 && x.WorkflowName == "DB Workflow 2 Updated"))), Times.Once);
            _workflowRepositoryMock.Verify(repo => repo.DeleteWorkflowsAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 0)), Times.Never);
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldHandleEmptyDatabaseWorkflows()
        {
            // Arrange
            var apiWorkflows = new List<Workflow>
            {
                new Workflow { WorkflowID = 1, WorkflowName = "API Workflow 1", IsActive = true }
            };
            var dbWorkflows = new List<Workflow>();

            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowsAsync())
                .ReturnsAsync(dbWorkflows);

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.AddWorkflowsAsync(It.IsAny<IEnumerable<Workflow>>()), Times.Once);
            _workflowRepositoryMock.Verify(repo => repo.UpdateWorkflowsAsync(It.IsAny<IEnumerable<Workflow>>()), Times.Never);
            _workflowRepositoryMock.Verify(repo => repo.DeleteWorkflowsAsync(It.IsAny<IEnumerable<int>>()), Times.Never);
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldHandleEmptyApiWorkflows()
        {
            // Arrange
            var apiWorkflows = new List<Workflow>();
            var dbWorkflows = new List<Workflow>
    {
        new Workflow { WorkflowID = 1, WorkflowName = "DB Workflow 1", IsActive = true }
    };

            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ReturnsAsync(apiWorkflows);
            _workflowRepositoryMock.Setup(repo => repo.GetWorkflowsAsync())
                .ReturnsAsync(dbWorkflows);

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _workflowRepositoryMock.Verify(repo => repo.AddWorkflowsAsync(It.IsAny<IEnumerable<Workflow>>()), Times.Never);
            _workflowRepositoryMock.Verify(repo => repo.UpdateWorkflowsAsync(It.IsAny<IEnumerable<Workflow>>()), Times.Never);
            _workflowRepositoryMock.Verify(repo => repo.DeleteWorkflowsAsync(It.Is<IEnumerable<int>>(ids => ids.Count() == 1 && ids.Contains(1))), Times.Once);
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldLogError_WhenExceptionOccurs()
        {
            // Arrange
            _externalWorkflowServiceMock.Setup(service => service.GetWorkflowsAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
