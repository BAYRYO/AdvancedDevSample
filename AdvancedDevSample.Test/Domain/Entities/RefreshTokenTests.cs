using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Test.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_GeneratesPlainTextTokenThatMatchesHash()
    {
        var refreshToken = new RefreshToken(Guid.NewGuid());

        var plainText = refreshToken.GetPlainTextTokenOrThrow();

        Assert.True(refreshToken.Matches(plainText));
        Assert.False(refreshToken.Matches("invalid-token"));
    }

    [Fact]
    public void ReconstitutedToken_GetPlainTextTokenOrThrow_ThrowsInvalidOperationException()
    {
        var reconstituted = new RefreshToken(
            id: Guid.NewGuid(),
            tokenHash: RefreshToken.HashToken("known-token"),
            userId: Guid.NewGuid(),
            expiresAt: DateTime.UtcNow.AddDays(1),
            createdAt: DateTime.UtcNow,
            isRevoked: false,
            revokedAt: null);

        Assert.Throws<InvalidOperationException>(() => reconstituted.GetPlainTextTokenOrThrow());
    }
}
