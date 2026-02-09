using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.ValueObjects;
using AdvancedDevSample.Infrastructure.Persistence.Entities;

namespace AdvancedDevSample.Infrastructure.Persistence.Mappers;

public static class ProductMapper
{
    public static Product ToDomain(ProductEntity entity)
    {
        Sku? sku = null;
        if (!string.IsNullOrEmpty(entity.Sku))
        {
            sku = new Sku(entity.Sku);
        }

        return new Product(new Product.ReconstitutionData
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            Sku = sku,
            Stock = entity.Stock,
            Description = entity.Description,
            CategoryId = entity.CategoryId,
            DiscountPercentage = entity.DiscountPercentage,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        });
    }

    public static ProductEntity ToEntity(Product product)
    {
        return new ProductEntity
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsActive = product.IsActive,
            Sku = product.Sku?.Value,
            Stock = product.Stock.Quantity,
            CategoryId = product.CategoryId,
            DiscountPercentage = product.CurrentDiscount?.Percentage,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
