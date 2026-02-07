using AdvancedDevSample.Domain.Enums;

namespace AdvancedDevSample.Infrastructure.Persistence.Entities;

public class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }

    // Navigation property
    public UserEntity? User { get; set; }
}
