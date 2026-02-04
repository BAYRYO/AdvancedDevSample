using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public record ProductSearchCriteria(
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);

public interface IProductRepository
{
    // Existing sync methods for backward compatibility
    Product? GetById(Guid id);
    void Save(Product product);

    // New async methods
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetBySkuAsync(string sku);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(ProductSearchCriteria criteria);
    Task SaveAsync(Product product);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsWithSkuAsync(string sku, Guid? excludeProductId = null);
}
