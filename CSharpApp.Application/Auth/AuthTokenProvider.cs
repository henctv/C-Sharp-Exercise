using System.Net.Http.Json;

namespace CSharpApp.Application.Auth;

public sealed class AuthTokenProvider(
    IHttpClientFactory httpClientFactory,
    IOptions<RestApiSettings> settings,
    ILogger<AuthTokenProvider> logger
) : IAuthTokenProvider
{
    public const string AuthClientName = "AuthClient";

    private string? _accessToken;

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_accessToken is not null)
        {
            return _accessToken;
        }

        var restApi = settings.Value;
        var client = httpClientFactory.CreateClient(AuthClientName);

        var response = await client.PostAsJsonAsync(
            restApi.Auth!,
            new
            {
                email = restApi.Username,
                password = restApi.Password
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthTokenDto>(cancellationToken);

        _accessToken = token?.AccessToken
            ?? throw new InvalidOperationException("Auth response did not contain an access token.");

        logger.LogInformation("Obtained new JWT access token from downstream API");
        return _accessToken;
    }

    public void InvalidateToken() => _accessToken = null;
}
