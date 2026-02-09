using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using AdvancedDevSample.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfUserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public EfUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        UserEntity? entity = await _context.Users.FindAsync(id);
        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        UserEntity? entity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        return entity == null ? null : UserMapper.ToDomain(entity);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        return await _context.Users.AnyAsync(u => u.Email == normalizedEmail);
    }

    public async Task SaveAsync(User user)
    {
        UserEntity entity = UserMapper.ToEntity(user);
        UserEntity? existing = await _context.Users.FindAsync(user.Id);

        if (existing == null)
        {
            _context.Users.Add(entity);
        }
        else
        {
            existing.Email = entity.Email;
            existing.PasswordHash = entity.PasswordHash;
            existing.FirstName = entity.FirstName;
            existing.LastName = entity.LastName;
            existing.Role = entity.Role;
            existing.IsActive = entity.IsActive;
            existing.UpdatedAt = entity.UpdatedAt;
            existing.LastLoginAt = entity.LastLoginAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        List<UserEntity> entities = await _context.Users
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(UserMapper.ToDomain);
    }

    public Task<int> GetCountAsync() => _context.Users.CountAsync();
}
