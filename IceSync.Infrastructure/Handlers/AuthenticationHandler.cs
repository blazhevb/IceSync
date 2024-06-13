using IceSync.Infrastructure.Services;
using System.Net.Http.Headers;

namespace IceSync.Infrastructure.Handlers;

public class AuthenticationHandler(UniversalLoaderTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string token = await tokenProvider.GetTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}