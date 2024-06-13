using IceSync.Infrastructure.ApiClients;
using IceSync.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace IceSync.Infrastructure.Services
{
    public class UniversalLoaderTokenProvider
    {
        private const int LeadTimeSeconds = 30;

        private readonly UniversalLoaderAPIClient _apiClient;
        private readonly CredentialsOptions _credentials;
        private readonly ILogger<UniversalLoaderTokenProvider> _logger;
        private readonly IMemoryCache _cache;

        public UniversalLoaderTokenProvider(
            UniversalLoaderAPIClient apiClient,
            IOptions<CredentialsOptions> options,
            ILogger<UniversalLoaderTokenProvider> logger,
            IMemoryCache cache)
        {
            _apiClient = apiClient;
            _credentials = options.Value;
            _logger = logger;
            _cache = cache;
        }

        public async Task<string> GetTokenAsync()
        {
            string? tokenRes = await _cache.GetOrCreateAsync("AccessToken", async entry =>
                {
                    string token = await RetrieveTokenAsync(entry);
                    return token;
                });

            return tokenRes;
        }

        private async Task<string> RetrieveTokenAsync(ICacheEntry entry)
        {
            _logger.LogInformation("Token is expired or not present. Requesting a new token.");

            ApiCredentials credentials = new ApiCredentials
            {
                ApiCompanyId = _credentials.CompanyID,
                ApiUserId = _credentials.UserID,
                ApiUserSecret = _credentials.UserSecret
            };

            try
            {
                Response response = await _apiClient.V2AuthenticateAsync(credentials);

                if(response == null || string.IsNullOrEmpty(response.Access_token))
                {
                    _logger.LogError("Failed to acquire a new token. The response was null or the access token was empty.");
                    throw new InvalidOperationException("Failed to acquire a new token.");
                }

                string token = response.Access_token;
                SetTokenCache(entry, token, response.Expires_in);

                _logger.LogInformation($"New token acquired and stored. Token valid until {entry.AbsoluteExpiration:O}.");
                return token;
            }
            catch(ApiException ex)
            {
                _logger.LogError(ex, "API error occurred while requesting a new token.");
                throw;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occurred while requesting a new token.");
                throw;
            }
        }

        private void SetTokenCache(ICacheEntry entry, string token, int expirationSeconds)
        {
            if(expirationSeconds <= 0)
            {
                string err = "Token expiration time is not defined or invalid.";
                _logger.LogError(err);
                throw new ArgumentException(err);
            }

            DateTime currentTime = DateTime.UtcNow;
            DateTime expiryTime = currentTime.AddSeconds(expirationSeconds - LeadTimeSeconds);

            entry.AbsoluteExpiration = expiryTime;
            entry.Value = token;
        }
    }
}
