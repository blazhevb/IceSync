using IceSync.Infrastructure.ApiClients;
using IceSync.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceSync.Infrastructure.Services;

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
        if(!_cache.TryGetValue("AccessToken", out string token))
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
                    throw new Exception("Failed to acquire a new token.");
                }

                token = response.Access_token;
                SetTokenCache(token, response.Expires_in);

                _logger.LogInformation($"New token acquired and stored. Token valid until {_cache.Get<DateTime>("TokenExpiryTime"):O}.");
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

        return token;
    }

    private void SetTokenCache(string token, int expirationSeconds)
    {
        if(expirationSeconds <= 0)
        {
            string err = "Token expiration time is not defined or invalid.";
            _logger.LogError(err);
            throw new ArgumentException(err);
        }

        DateTime currentTime = DateTime.UtcNow;
        DateTime expiryTime = currentTime.AddSeconds(expirationSeconds - LeadTimeSeconds);

        _cache.Set("AccessToken", token, expiryTime);
        _cache.Set("TokenExpiryTime", expiryTime, expiryTime);
    }
}
