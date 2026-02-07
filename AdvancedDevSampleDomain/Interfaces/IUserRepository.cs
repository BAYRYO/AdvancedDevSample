using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task SaveAsync(User user);
    Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<int> GetCountAsync();
}
