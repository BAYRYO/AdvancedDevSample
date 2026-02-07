using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Infrastructure.Persistence.Entities;

namespace AdvancedDevSample.Infrastructure.Persistence.Mappers;

public static class UserMapper
{
    public static User ToDomain(UserEntity entity)
    {
        return new User(
            id: entity.Id,
            email: entity.Email,
            passwordHash: entity.PasswordHash,
            firstName: entity.FirstName,
            lastName: entity.LastName,
            role: (UserRole)entity.Role,
            isActive: entity.IsActive,
            createdAt: entity.CreatedAt,
            updatedAt: entity.UpdatedAt,
            lastLoginAt: entity.LastLoginAt);
    }

    public static UserEntity ToEntity(User user)
    {
        return new UserEntity
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = (int)user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
