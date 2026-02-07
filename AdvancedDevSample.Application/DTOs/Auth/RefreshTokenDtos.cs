using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Auth;

public record RefreshTokenRequest(
    [Required]
    [MinLength(32)]
    string RefreshToken);

public record AuthResponseWithRefreshToken(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserResponse User);
