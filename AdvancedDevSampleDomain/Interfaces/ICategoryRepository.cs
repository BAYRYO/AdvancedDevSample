using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<IReadOnlyList<Category>> GetActiveAsync();
    Task SaveAsync(Category category);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
