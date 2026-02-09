using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _store = [];

    public Task<Category?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out Category? category);
        return Task.FromResult(category);
    }

    public Task<IReadOnlyList<Category>> GetAllAsync()
    {
        IReadOnlyList<Category> result = [.. _store.Values.OrderBy(c => c.Name)];
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Category>> GetActiveAsync()
    {
        IReadOnlyList<Category> result =
        [
            .. _store.Values
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
        ];
        return Task.FromResult(result);
    }

    public Task SaveAsync(Category category)
    {
        _store[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_store.ContainsKey(id));

    public void Seed(Category category) => _store[category.Id] = category;

    public void Clear() => _store.Clear();
}
