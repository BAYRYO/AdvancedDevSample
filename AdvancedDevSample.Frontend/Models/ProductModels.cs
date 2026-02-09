using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Frontend.Models;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Sku,
    decimal Price,
    decimal EffectivePrice,
    decimal? DiscountPercentage,
    int Stock,
    Guid? CategoryId,
    string? CategoryName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateProductRequest(
    [Required] string Name,
    [Required] string Sku,
    [Required] decimal Price,
    int Stock = 0,
    string? Description = null,
    Guid? CategoryId = null);

public record ProductSearchRequest(
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);

public record ApplyDiscountRequest([Required] decimal Percentage, string? Reason = null);

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public class ProductSearchModel
{
    public string? Name { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public ProductSearchRequest ToRequest() => new(Name, MinPrice, MaxPrice, CategoryId, IsActive, Page, PageSize);
}

public class CreateProductFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Sku { get; set; } = string.Empty;

    [Required, Range(0.01, 9999999)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }

    public CreateProductRequest ToRequest() => new(Name, Sku, Price, Stock, Description, CategoryId);
}

public class ApplyDiscountFormModel
{
    [Range(0.01, 100)]
    public decimal Percentage { get; set; }

    [MaxLength(300)]
    public string? Reason { get; set; }

    public ApplyDiscountRequest ToRequest() => new(Percentage, Reason);
}
