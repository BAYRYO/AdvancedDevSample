using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly Dictionary<Guid, RefreshToken> _tokens = [];

    public Task<RefreshToken?> GetByTokenAsync(string token)
    {
        RefreshToken? refreshToken = _tokens.Values.FirstOrDefault(t => t.Matches(token));
        return Task.FromResult(refreshToken);
    }

    public Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        IEnumerable<RefreshToken> tokens = _tokens.Values.Where(t => t.UserId == userId);
        return Task.FromResult(tokens);
    }

    public Task SaveAsync(RefreshToken refreshToken)
    {
        _tokens[refreshToken.Id] = refreshToken;
        return Task.CompletedTask;
    }

    public Task RevokeAllForUserAsync(Guid userId)
    {
        List<RefreshToken> userTokens = [.. _tokens.Values.Where(t => t.UserId == userId)];
        foreach (RefreshToken token in userTokens)
        {
            token.Revoke();
        }
        return Task.CompletedTask;
    }

    public void Clear() => _tokens.Clear();
}
