using System.Diagnostics.CodeAnalysis;

namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Represents a refresh token used for JWT token renewal.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; }
    public string? PlainTextToken { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    [SuppressMessage("Major Code Smell", "S1144", Justification = "Setter required for EF Core navigation materialization.")]
    public User? User { get; private set; }

    // Constructor for creating new refresh tokens
    public RefreshToken(Guid userId, int expirationDays = 7)
    {
        Id = Guid.NewGuid();
        PlainTextToken = GenerateToken();
        TokenHash = HashToken(PlainTextToken);
        UserId = userId;
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    // Constructor for reconstitution from persistence
    public RefreshToken(
        Guid id,
        string tokenHash,
        Guid userId,
        DateTime expiresAt,
        DateTime createdAt,
        bool isRevoked,
        DateTime? revokedAt)
    {
        Id = id;
        TokenHash = tokenHash;
        PlainTextToken = null;
        UserId = userId;
        ExpiresAt = expiresAt;
        CreatedAt = createdAt;
        IsRevoked = isRevoked;
        RevokedAt = revokedAt;
    }

    public bool IsValid => !IsRevoked && ExpiresAt > DateTime.UtcNow;

    public bool Matches(string candidateToken) => TokenHash == HashToken(candidateToken);

    public string GetPlainTextTokenOrThrow()
    {
        return PlainTextToken
            ?? throw new InvalidOperationException(
                "Plain text refresh token is only available when the token is first generated.");
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }

    public static string HashToken(string token)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token);
        byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static string GenerateToken()
    {
        byte[] randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
