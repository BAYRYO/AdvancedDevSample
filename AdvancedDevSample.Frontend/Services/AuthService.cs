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

    public Task<StoredAuthSession?> GetSessionAsync() => _tokenStore.GetAsync();

    public async Task LoginAsync(LoginRequest request)
    {
        HttpClient client = _httpClientFactory.CreateClient("ApiNoAuth");
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/login", request);
        AuthResponseWithRefreshToken auth = await ReadRequiredAsync<AuthResponseWithRefreshToken>(response);
        StoredAuthSession session = StoredAuthSession.FromAuthResponse(auth);

        await _tokenStore.SaveAsync(session);
        await _authStateProvider.SetAuthenticatedUserAsync(session);
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        HttpClient client = _httpClientFactory.CreateClient("ApiNoAuth");
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/register", request);
        AuthResponseWithRefreshToken auth = await ReadRequiredAsync<AuthResponseWithRefreshToken>(response);
        StoredAuthSession session = StoredAuthSession.FromAuthResponse(auth);

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
            StoredAuthSession? existing = await _tokenStore.GetAsync();
            if (existing is null || !existing.CanRefresh)
            {
                await LogoutAsync();
                return null;
            }

            if (existing.ExpiresAt > DateTime.UtcNow.AddSeconds(30))
            {
                return existing;
            }

            HttpClient client = _httpClientFactory.CreateClient("ApiNoAuth");
            HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(existing.RefreshToken));

            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return null;
            }

            AuthResponseWithRefreshToken? auth = await response.Content.ReadFromJsonAsync<AuthResponseWithRefreshToken>();
            if (auth is null)
            {
                await LogoutAsync();
                return null;
            }

            StoredAuthSession refreshed = StoredAuthSession.FromAuthResponse(auth);
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
            string message = await ExtractErrorAsync(response);
            throw new ApiException(message, (int)response.StatusCode);
        }

        T? data = await response.Content.ReadFromJsonAsync<T>();
        return data ?? throw new ApiException("API returned an empty response.", (int)response.StatusCode);
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return $"Request failed with status {(int)response.StatusCode}.";
        }

        if (body.Contains("\"detail\"", StringComparison.OrdinalIgnoreCase))
        {
            string? detail = body.Split("\"detail\":", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (!string.IsNullOrWhiteSpace(detail))
            {
                return body;
            }
        }

        return body;
    }
}
