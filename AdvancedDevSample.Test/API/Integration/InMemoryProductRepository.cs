using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryProductRepository : IProductRepository
{
    private readonly Dictionary<Guid, Product> _store = [];

    // Existing sync methods
    public Product? GetById(Guid id)
        => _store.TryGetValue(id, out Product? product) ? product : null;

    public void Save(Product product) => _store[product.Id] = product;

    // New async methods
    public Task<Product?> GetByIdAsync(Guid id)
        => Task.FromResult(GetById(id));

    public Task<Product?> GetBySkuAsync(string sku)
    {
        string normalized = sku.ToUpperInvariant();
        Product? product = _store.Values.FirstOrDefault(p => p.Sku?.Value == normalized);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>> GetAllAsync()
    {
        IReadOnlyList<Product> result = [.. _store.Values];
        return Task.FromResult(result);
    }

    public Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(ProductSearchCriteria criteria)
    {
        IEnumerable<Product> query = _store.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            query = query.Where(p => p.Name.Contains(criteria.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
        }

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == criteria.CategoryId.Value);
        }

        if (criteria.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == criteria.IsActive.Value);
        }

        int totalCount = query.Count();
        var items =
        [
            .. query
                .OrderBy(p => p.Name)
                .Skip((criteria.Page - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
        ];

        return Task.FromResult(((IReadOnlyList<Product>)items, totalCount));
    }

    public Task SaveAsync(Product product)
    {
        Save(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _store.Remove(id);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsWithSkuAsync(string sku, Guid? excludeProductId = null)
    {
        string normalized = sku.ToUpperInvariant();
        bool exists = _store.Values.Any(p =>
            p.Sku?.Value == normalized &&
            (!excludeProductId.HasValue || p.Id != excludeProductId.Value));
        return Task.FromResult(exists);
    }

    // Test helper methods
    public void Seed(Product product) => _store[product.Id] = product;

    public void Clear() => _store.Clear();
}
