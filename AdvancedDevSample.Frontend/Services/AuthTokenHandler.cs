using System.Net;
using System.Net.Http.Headers;
using AdvancedDevSample.Frontend.Models;

namespace AdvancedDevSample.Frontend.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;
    private readonly AuthService _authService;

    public AuthTokenHandler(TokenStore tokenStore, AuthService authService)
    {
        _tokenStore = tokenStore;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (IsAuthEndpoint(request.RequestUri))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        StoredAuthSession? session = await _tokenStore.GetAsync();
        if (session is not null)
        {
            if (session.ExpiresAt <= DateTime.UtcNow.AddSeconds(30))
            {
                session = await _authService.TryRefreshTokenAsync();
            }

            if (session is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
            }
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _authService.LogoutAsync();
        }

        return response;
    }

    private static bool IsAuthEndpoint(Uri? uri)
    {
        if (uri is null)
        {
            return false;
        }

        string path = uri.AbsolutePath;
        return path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase)
            || path.Equals("/api/auth/refresh", StringComparison.OrdinalIgnoreCase);
    }
}
