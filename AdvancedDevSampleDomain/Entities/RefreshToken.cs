namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Represents a refresh token used for JWT token renewal.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    public User? User { get; private set; }

    // Constructor for creating new refresh tokens
    public RefreshToken(Guid userId, int expirationDays = 7)
    {
        Id = Guid.NewGuid();
        Token = GenerateToken();
        UserId = userId;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    // Constructor for reconstitution from persistence
    public RefreshToken(
        Guid id,
        string token,
        Guid userId,
        DateTime expiresAt,
        DateTime createdAt,
        bool isRevoked,
        DateTime? revokedAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsRevoked = isRevoked;
        RevokedAt = revokedAt;
    }

    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
