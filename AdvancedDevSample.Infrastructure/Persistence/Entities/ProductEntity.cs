namespace AdvancedDevSample.Infrastructure.Persistence.Entities;

public class ProductEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string? Sku { get; set; }
    public int Stock { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public CategoryEntity? Category { get; set; }
}
