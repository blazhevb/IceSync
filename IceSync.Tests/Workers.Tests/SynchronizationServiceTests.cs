using IceSync.Domain.Contracts.Managers;
using IceSync.Workers.Configurations;
using IceSync.Workers.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IceSync.Tests.Workers.Services
{
    [TestClass]
    public class SynchronizationServiceTests
    {
        private Mock<ILogger<SynchronizationService>> _loggerMock;
        private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
        private Mock<IOptions<SynchronizationWorkerOptions>> _optionsMock;
        private Mock<IServiceScope> _serviceScopeMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IWorkflowManager> _workflowManagerMock;
        private SynchronizationWorkerOptions _options;
        private SynchronizationService _sut;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<SynchronizationService>>();
            _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            _optionsMock = new Mock<IOptions<SynchronizationWorkerOptions>>();
            _serviceScopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _workflowManagerMock = new Mock<IWorkflowManager>();

            _options = new SynchronizationWorkerOptions { IntervalSeconds = 60 };
            _optionsMock.Setup(o => o.Value).Returns(_options);

            _serviceProviderMock.Setup(sp => sp.GetService(typeof(IWorkflowManager))).Returns(_workflowManagerMock.Object);
            _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _serviceScopeFactoryMock.Setup(s => s.CreateScope()).Returns(_serviceScopeMock.Object);

            _sut = new SynchronizationService(
                _loggerMock.Object,
                _serviceScopeFactoryMock.Object,
                _optionsMock.Object);
        }

        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldLogInformationAndSynchronizeWorkflows()
        {
            // Arrange
            _workflowManagerMock.Setup(m => m.SynchronizeWorkflowsAsync()).Returns(Task.CompletedTask);

            // Act
            await _sut.SynchronizeWorkflowsAsync();

            // Assert
            _workflowManagerMock.Verify(m => m.SynchronizeWorkflowsAsync(), Times.Once);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }


        [TestMethod]
        public async Task SynchronizeWorkflowsAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            _workflowManagerMock.Setup(m => m.SynchronizeWorkflowsAsync()).ThrowsAsync(new Exception("Test exception"));

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

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogStartingAndStoppingInformation()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1)); // To stop the while loop quickly

            // Act
            await _sut.StartAsync(cancellationTokenSource.Token);
            await _sut.StopAsync(cancellationTokenSource.Token);

            // Assert
            _loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task ExecuteAsync_ShouldLogError_WhenExceptionIsThrownDuringSynchronization()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1)); // To stop the while loop quickly
            _workflowManagerMock.Setup(m => m.SynchronizeWorkflowsAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            await _sut.StartAsync(cancellationTokenSource.Token);
            await _sut.StopAsync(cancellationTokenSource.Token);

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
