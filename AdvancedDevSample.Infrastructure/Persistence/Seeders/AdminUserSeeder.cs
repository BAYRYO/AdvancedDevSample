using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds a default admin user for development and testing.
/// </summary>
public class AdminUserSeeder : ISeeder
{
    // Run first to ensure admin exists before other seeders
    public int Order => 0;

    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        // Check if any admin user already exists
        int adminRoleValue = (int)UserRole.Admin;
        bool adminExists = await context.Users
            .AnyAsync(u => u.Role == adminRoleValue, cancellationToken);

        if (adminExists)
        {
            return;
        }

        // Skip admin creation when credentials are not configured.
        string? email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        string? password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }
        string firstName = "Admin";
        string lastName = "User";

        // Hash the password using BCrypt directly (matching PasswordHasher implementation)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var adminUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = (int)UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = null
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync(cancellationToken);
    }
}


