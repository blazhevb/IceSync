using IceSync.Infrastructure.ApiClients;
using IceSync.Infrastructure.Configurations;
using IceSync.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace IceSync.Tests.Infrastructure.Tests
{
    [TestClass]
    public class UniversalLoaderTokenProviderTests
    {
        private Mock<IOptions<CredentialsOptions>> _optionsMock;
        private Mock<ILogger<UniversalLoaderTokenProvider>> _loggerMock;
        private Mock<IMemoryCache> _cacheMock;
        private UniversalLoaderTokenProvider _sut;

        public class UniversalLoaderAPIClientStub : UniversalLoaderAPIClient
        {
            private readonly Response _response;

            public UniversalLoaderAPIClientStub(Response response, string baseUrl, HttpClient httpClient)
                : base(baseUrl, httpClient)
            {
                _response = response;
            }

            public override Task<Response> V2AuthenticateAsync(ApiCredentials credentials)
            {
                return Task.FromResult(_response);
            }
        }

        [TestInitialize]
        public void Setup()
        {
            _optionsMock = new Mock<IOptions<CredentialsOptions>>();
            _loggerMock = new Mock<ILogger<UniversalLoaderTokenProvider>>();
            _cacheMock = new Mock<IMemoryCache>();

            _optionsMock.Setup(o => o.Value).Returns(new CredentialsOptions
            {
                CompanyID = "TestCompany",
                UserID = "TestUser",
                UserSecret = "TestSecret"
            });
        }

        [TestMethod]
        public async Task GetTokenAsync_ShouldReturnToken_FromCache()
        {
            // Arrange
            var cachedToken = "cached_token";
            object token = cachedToken;
            _cacheMock.Setup(c => c.TryGetValue("AccessToken", out token)).Returns(true);

            var apiClientStub = new UniversalLoaderAPIClientStub(new Response(), "", null);

            _sut = new UniversalLoaderTokenProvider(apiClientStub, _optionsMock.Object, _loggerMock.Object, _cacheMock.Object);

            // Act
            var result = await _sut.GetTokenAsync();

            // Assert
            Assert.AreEqual(cachedToken, result);
        }

        [TestMethod]
        public async Task GetTokenAsync_ShouldRequestNewToken_WhenTokenNotInCache()
        {
            // Arrange
            var newToken = "new_token";
            var response = new Response
            {
                Access_token = newToken,
                Expires_in = 3600
            };

            object token = null;
            _cacheMock.Setup(c => c.TryGetValue("AccessToken", out token)).Returns(false);
            _cacheMock.Setup(c => c.CreateEntry(It.IsAny<object>())).Returns(Mock.Of<ICacheEntry>());

            var apiClientStub = new UniversalLoaderAPIClientStub(response, "", null);

            _sut = new UniversalLoaderTokenProvider(apiClientStub, _optionsMock.Object, _loggerMock.Object, _cacheMock.Object);

            // Act
            var result = await _sut.GetTokenAsync();

            // Assert
            Assert.AreEqual(newToken, result);
        }

        [TestMethod]
        public async Task GetTokenAsync_ShouldLogError_WhenApiResponseIsInvalid()
        {
            // Arrange
            var response = new Response { Access_token = null, Expires_in = 3600 };

            object token = null;
            _cacheMock.Setup(c => c.TryGetValue("AccessToken", out token)).Returns(false);

            var apiClientStub = new UniversalLoaderAPIClientStub(response, "", null);

            _sut = new UniversalLoaderTokenProvider(apiClientStub, _optionsMock.Object, _loggerMock.Object, _cacheMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _sut.GetTokenAsync());
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeast(2));
        }

        [TestMethod]
        public async Task GetTokenAsync_ShouldLogError_WhenApiClientThrowsException()
        {
            // Arrange
            object token = null;
            _cacheMock.Setup(c => c.TryGetValue("AccessToken", out token)).Returns(false);

            var apiClientStub = new UniversalLoaderAPIClientStub(new Response(), "", null);
            var mockApiClient = new Mock<UniversalLoaderAPIClientStub>(new Response(), "", null);
            mockApiClient.Setup(a => a.V2AuthenticateAsync(It.IsAny<ApiCredentials>())).ThrowsAsync(new Exception("API error"));

            _sut = new UniversalLoaderTokenProvider(mockApiClient.Object, _optionsMock.Object, _loggerMock.Object, _cacheMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(async () => await _sut.GetTokenAsync());
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
