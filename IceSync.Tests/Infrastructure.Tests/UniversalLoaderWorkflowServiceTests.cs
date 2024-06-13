using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IceSync.Infrastructure.Services;
using IceSync.Infrastructure.ApiClients;
using IceSync.Domain.Contracts;
using IceSync.Domain.Entities;
using IceSync.Infrastructure.Mappers;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IceSync.Infrastructure.Contracts;
using System.Net.Http;

namespace IceSync.Tests.Infrastructure.Tests
{
    [TestClass]
    public class UniversalLoaderWorkflowServiceTests
    {
        private Mock<IWorkflowMapper> _mapperMock;
        private Mock<ILogger<UniversalLoaderWorkflowService>> _loggerMock;
        private UniversalLoaderWorkflowService _sut;

        private class UniversalLoaderAPIClientStub : UniversalLoaderAPIClient
        {
            private readonly ICollection<IceSync.Infrastructure.ApiClients.Workflow> _workflows;
            private readonly bool _throwException;
            private readonly bool _runWorkflowThrowsException;

            public UniversalLoaderAPIClientStub(ICollection<IceSync.Infrastructure.ApiClients.Workflow> workflows, bool throwException = false, bool runWorkflowThrowsException = false)
                : base("", null)
            {
                _workflows = workflows;
                _throwException = throwException;
                _runWorkflowThrowsException = runWorkflowThrowsException;
            }

            public override Task<ICollection<IceSync.Infrastructure.ApiClients.Workflow>> WorkflowsAsync()
            {
                if(_throwException)
                {
                    throw new ApiException("API error", 404, "", null, null);
                }
                return Task.FromResult(_workflows);
            }
         
            public override Task WorkflowsRunAsync(int workflowId, bool? waitOutput, bool? decodeOutputJsonString)
            {
                if(_runWorkflowThrowsException)
                {
                    throw new ApiException("API error", 404, "", null, null);
                }
                return Task.CompletedTask;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _mapperMock = new Mock<IWorkflowMapper>();
            _loggerMock = new Mock<ILogger<UniversalLoaderWorkflowService>>();
        }

        [TestMethod]
        public async Task GetWorkflowsAsync_ShouldReturnMappedWorkflows()
        {
            // Arrange
            var apiWorkflows = new List<IceSync.Infrastructure.ApiClients.Workflow>
            {
                new IceSync.Infrastructure.ApiClients.Workflow { Id = 1, Name = "API Workflow 1", IsActive = true }
            };
            var domainWorkflow = new Domain.Entities.Workflow { WorkflowID = 1, WorkflowName = "API Workflow 1", IsActive = true };

            var apiClientStub = new UniversalLoaderAPIClientStub(apiWorkflows);
            _mapperMock.Setup(m => m.MapToDomainWorkflow(It.IsAny<IceSync.Infrastructure.ApiClients.Workflow>())).Returns(domainWorkflow);

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await _sut.GetWorkflowsAsync();

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(domainWorkflow.WorkflowID, result.First().WorkflowID);
        }

        [TestMethod]
        public async Task GetWorkflowsAsync_ShouldThrowException_WhenApiExceptionOccurs()
        {
            // Arrange
            var apiClientStub = new UniversalLoaderAPIClientStub(new List<IceSync.Infrastructure.ApiClients.Workflow>(), throwException: true);

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ApiException>(async () => await _sut.GetWorkflowsAsync());
        }

        [TestMethod]
        public async Task GetWorkflowsAsync_ShouldLogInformationMessages()
        {
            // Arrange
            var apiWorkflows = new List<IceSync.Infrastructure.ApiClients.Workflow>
            {
                new IceSync.Infrastructure.ApiClients.Workflow { Id = 1, Name = "API Workflow 1", IsActive = true }
            };
            var domainWorkflow = new Domain.Entities.Workflow { WorkflowID = 1, WorkflowName = "API Workflow 1", IsActive = true };

            var apiClientStub = new UniversalLoaderAPIClientStub(apiWorkflows);
            _mapperMock.Setup(m => m.MapToDomainWorkflow(It.IsAny<IceSync.Infrastructure.ApiClients.Workflow>())).Returns(domainWorkflow);

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act
            await _sut.GetWorkflowsAsync();

            // Assert
            _loggerMock.Verify(log => log.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Fetching workflows from UniversalLoader API.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);

            _loggerMock.Verify(log => log.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Successfully fetched workflows.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task RunWorkflowAsync_ShouldReturnTrue_WhenWorkflowRunsSuccessfully()
        {
            // Arrange
            var workflowId = 1;
            var apiClientStub = new UniversalLoaderAPIClientStub(new List<IceSync.Infrastructure.ApiClients.Workflow>());

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await _sut.RunWorkflowAsync(workflowId);

            // Assert
            Assert.IsTrue(result);
            _loggerMock.Verify(l => l.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.IsAny<It.IsAnyType>(),
               null,
               It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task RunWorkflowAsync_ShouldReturnFalse_WhenApiExceptionOccurs()
        {
            // Arrange
            var workflowId = 1;
            var apiClientStub = new UniversalLoaderAPIClientStub(new List<IceSync.Infrastructure.ApiClients.Workflow>(), runWorkflowThrowsException: true);

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act
            var result = await _sut.RunWorkflowAsync(workflowId);

            // Assert
            Assert.IsFalse(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task RunWorkflowAsync_ShouldLogInformationMessages()
        {
            // Arrange
            var workflowId = 1;
            var apiClientStub = new UniversalLoaderAPIClientStub(new List<IceSync.Infrastructure.ApiClients.Workflow>());

            _sut = new UniversalLoaderWorkflowService(apiClientStub, _mapperMock.Object, _loggerMock.Object);

            // Act
            await _sut.RunWorkflowAsync(workflowId);

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }
    }
}
