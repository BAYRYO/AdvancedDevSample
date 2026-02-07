using System.Net.Http;
using System.Net.Http.Json;
using AdvancedDevSample.Frontend.Models;

namespace AdvancedDevSample.Frontend.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TokenStore _tokenStore;
    private readonly FrontendAuthStateProvider _authStateProvider;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthService(
        IHttpClientFactory httpClientFactory,
        TokenStore tokenStore,
        FrontendAuthStateProvider authStateProvider)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStore = tokenStore;
        _authStateProvider = authStateProvider;
    }

    public async Task<StoredAuthSession?> GetSessionAsync()
    {
        return await _tokenStore.GetAsync();
    }

    public async Task LoginAsync(LoginRequest request)
    {
        var client = _httpClientFactory.CreateClient("ApiNoAuth");
        var response = await client.PostAsJsonAsync("/api/auth/login", request);
        var auth = await ReadRequiredAsync<AuthResponseWithRefreshToken>(response);
        var session = StoredAuthSession.FromAuthResponse(auth);

        await _tokenStore.SaveAsync(session);
        await _authStateProvider.SetAuthenticatedUserAsync(session);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var client = _httpClientFactory.CreateClient("ApiNoAuth");
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        var auth = await ReadRequiredAsync<AuthResponseWithRefreshToken>(response);
        var session = StoredAuthSession.FromAuthResponse(auth);

        await _tokenStore.SaveAsync(session);
        await _authStateProvider.SetAuthenticatedUserAsync(session);
    }

    public async Task LogoutAsync()
    {
        await _tokenStore.ClearAsync();
        await _authStateProvider.MarkLoggedOutAsync();
    }

    public async Task<StoredAuthSession?> TryRefreshTokenAsync()
    {
        await _refreshLock.WaitAsync();
        try
        {
            var existing = await _tokenStore.GetAsync();
            if (existing is null || !existing.CanRefresh)
            {
                await LogoutAsync();
                return null;
            }

            if (existing.ExpiresAt > DateTime.UtcNow.AddSeconds(30))
            {
                return existing;
            }

            var client = _httpClientFactory.CreateClient("ApiNoAuth");
            var response = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(existing.RefreshToken));

            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return null;
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponseWithRefreshToken>();
            if (auth is null)
            {
                await LogoutAsync();
                return null;
            }

            var refreshed = StoredAuthSession.FromAuthResponse(auth);
            await _tokenStore.SaveAsync(refreshed);
            await _authStateProvider.SetAuthenticatedUserAsync(refreshed);
            return refreshed;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var message = await ExtractErrorAsync(response);
            throw new ApiException(message, (int)response.StatusCode);
        }

        var data = await response.Content.ReadFromJsonAsync<T>();
        if (data is null)
        {
            throw new ApiException("API returned an empty response.", (int)response.StatusCode);
        }

        return data;
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Request failed with status {(int)response.StatusCode}.";
        }

        if (body.Contains("\"detail\"", StringComparison.OrdinalIgnoreCase))
        {
            var detail = body.Split("\"detail\":", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (!string.IsNullOrWhiteSpace(detail))
            {
                return body;
            }
        }

        return body;
    }
}
