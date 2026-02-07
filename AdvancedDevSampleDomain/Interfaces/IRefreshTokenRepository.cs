using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId);
    Task SaveAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(Guid userId);
}
