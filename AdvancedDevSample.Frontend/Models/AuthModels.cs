using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Frontend.Models;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(100)] string FirstName,
    [Required, MaxLength(100)] string LastName);

public record RefreshTokenRequest(string RefreshToken);

public record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public record AuthResponseWithRefreshToken(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserResponse User);

public record StoredAuthSession(
    string Token,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserResponse User)
{
    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool CanRefresh => RefreshTokenExpiresAt > DateTime.UtcNow;

    public static StoredAuthSession FromAuthResponse(AuthResponseWithRefreshToken response)
    {
        return new StoredAuthSession(
            response.Token,
            response.ExpiresAt,
            response.RefreshToken,
            response.RefreshTokenExpiresAt,
            response.User);
    }
}

public class LoginFormModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RegisterFormModel
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
}
