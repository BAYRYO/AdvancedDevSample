using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public EfProductRepository(AppDbContext context)
    {
        _context = context;
    }

    // Existing sync methods for backward compatibility
    public Product? GetById(Guid id)
    {
        var entity = _context.Products.Find(id);
        return entity == null ? null : ProductMapper.ToDomain(entity);
    }

    public void Save(Product product)
    {
        var entity = ProductMapper.ToEntity(product);
        var existing = _context.Products.Find(product.Id);

        if (existing == null)
        {
            _context.Products.Add(entity);
        }
        else
        {
            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.Price = entity.Price;
            existing.IsActive = entity.IsActive;
            existing.Sku = entity.Sku;
            existing.Stock = entity.Stock;
            existing.CategoryId = entity.CategoryId;
            existing.DiscountPercentage = entity.DiscountPercentage;
            existing.UpdatedAt = entity.UpdatedAt;
        }

        _context.SaveChanges();
    }

    // New async methods
    public async Task<Product?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Products.FindAsync(id);
        return entity == null ? null : ProductMapper.ToDomain(entity);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        var normalizedSku = sku.ToUpperInvariant();
        var entity = await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == normalizedSku);
        return entity == null ? null : ProductMapper.ToDomain(entity);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync()
    {
        var entities = await _context.Products.ToListAsync();
        return entities.Select(ProductMapper.ToDomain).ToList();
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> SearchAsync(ProductSearchCriteria criteria)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.Name))
        {
            query = query.Where(p => p.Name.Contains(criteria.Name));
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

        var totalCount = await query.CountAsync();

        var entities = await query
            .OrderBy(p => p.Name)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync();

        var items = entities.Select(ProductMapper.ToDomain).ToList();
        return (items, totalCount);
    }

    public async Task SaveAsync(Product product)
    {
        var entity = ProductMapper.ToEntity(product);
        var existing = await _context.Products.FindAsync(product.Id);

        if (existing == null)
        {
            _context.Products.Add(entity);
        }
        else
        {
            existing.Name = entity.Name;
            existing.Description = entity.Description;
            existing.Price = entity.Price;
            existing.IsActive = entity.IsActive;
            existing.Sku = entity.Sku;
            existing.Stock = entity.Stock;
            existing.CategoryId = entity.CategoryId;
            existing.DiscountPercentage = entity.DiscountPercentage;
            existing.UpdatedAt = entity.UpdatedAt;
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity != null)
        {
            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsWithSkuAsync(string sku, Guid? excludeProductId = null)
    {
        var normalizedSku = sku.ToUpperInvariant();
        var query = _context.Products.Where(p => p.Sku == normalizedSku);

        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }
}
