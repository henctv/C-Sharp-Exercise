using System.Net;
using System.Net.Http.Headers;

namespace CSharpApp.Infrastructure.Configuration;

public sealed class AuthenticationDelegatingHandler(IAuthTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetAccessTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            tokenProvider.InvalidateToken();
        }

        return response;
    }
}
