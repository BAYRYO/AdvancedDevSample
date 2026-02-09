using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users = [];

    public Task<User?> GetByIdAsync(Guid id)
    {
        _users.TryGetValue(id, out User? user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        User? user = _users.Values.FirstOrDefault(u => u.Email == normalizedEmail);
        return Task.FromResult(user);
    }

    public Task<bool> ExistsByEmailAsync(string email)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        bool exists = _users.Values.Any(u => u.Email == normalizedEmail);
        return Task.FromResult(exists);
    }

    public Task SaveAsync(User user)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        IEnumerable<User> users = _users.Values
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        return Task.FromResult(users);
    }

    public Task<int> GetCountAsync() => Task.FromResult(_users.Count);

    public void Clear() => _users.Clear();
}

