namespace AdvancedDevSample.Application.DTOs.Auth;

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
