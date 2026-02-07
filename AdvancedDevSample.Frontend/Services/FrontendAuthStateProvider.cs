using System.Security.Claims;
using AdvancedDevSample.Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace AdvancedDevSample.Frontend.Services;

public class FrontendAuthStateProvider : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly TokenStore _tokenStore;
    private bool _initialized;

    public FrontendAuthStateProvider(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await EnsureSessionLoadedAsync();
        return new AuthenticationState(CreatePrincipal(session?.User));
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        var session = await _tokenStore.GetAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(session?.User))));
    }

    public Task SetAuthenticatedUserAsync(StoredAuthSession session)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(session.User))));
        return Task.CompletedTask;
    }

    public Task MarkLoggedOutAsync()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
        return Task.CompletedTask;
    }

    private async Task<StoredAuthSession?> EnsureSessionLoadedAsync()
    {
        _initialized = true;
        return await _tokenStore.GetAsync();
    }

    private static ClaimsPrincipal CreatePrincipal(UserResponse? user)
    {
        if (user is null)
        {
            return Anonymous;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new ClaimsPrincipal(identity);
    }
}
