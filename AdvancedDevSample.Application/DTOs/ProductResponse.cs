namespace AdvancedDevSample.Application.DTOs;

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
