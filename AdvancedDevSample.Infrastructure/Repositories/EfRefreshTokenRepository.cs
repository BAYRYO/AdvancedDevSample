using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public EfRefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        string tokenHash = RefreshToken.HashToken(token);
        RefreshTokenEntity? entity = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == tokenHash);

        return entity == null ? null : ToDomain(entity);
    }

    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
    {
        List<RefreshTokenEntity> entities = await _context.RefreshTokens
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return entities.Select(ToDomain);
    }

    public async Task SaveAsync(RefreshToken refreshToken)
    {
        RefreshTokenEntity entity = ToEntity(refreshToken);
        RefreshTokenEntity? existing = await _context.RefreshTokens.FindAsync(refreshToken.Id);

        if (existing == null)
        {
            _context.RefreshTokens.Add(entity);
        }
        else
        {
            existing.Token = entity.Token;
            existing.IsRevoked = entity.IsRevoked;
            existing.RevokedAt = entity.RevokedAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        List<RefreshTokenEntity> tokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync();

        foreach (RefreshTokenEntity token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private static RefreshToken ToDomain(RefreshTokenEntity entity)
    {
        return new RefreshToken(
            id: entity.Id,
            tokenHash: entity.Token,
            userId: entity.UserId,
            expiresAt: entity.ExpiresAt,
            createdAt: entity.CreatedAt,
            isRevoked: entity.IsRevoked,
            revokedAt: entity.RevokedAt);
    }

    private static RefreshTokenEntity ToEntity(RefreshToken domain)
    {
        return new RefreshTokenEntity
        {
            Id = domain.Id,
            Token = domain.TokenHash,
            UserId = domain.UserId,
            ExpiresAt = domain.ExpiresAt,
            CreatedAt = domain.CreatedAt,
            IsRevoked = domain.IsRevoked,
            RevokedAt = domain.RevokedAt
        };
    }
}
