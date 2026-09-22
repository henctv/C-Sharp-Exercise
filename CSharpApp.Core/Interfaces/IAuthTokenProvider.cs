namespace CSharpApp.Core.Interfaces;

public interface IAuthTokenProvider
{
    ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    void InvalidateToken();
}
