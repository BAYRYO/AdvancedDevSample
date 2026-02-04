namespace AdvancedDevSample.Application.DTOs;

public record PriceHistoryResponse(
    Guid Id,
    Guid ProductId,
    decimal OldPrice,
    decimal NewPrice,
    decimal? DiscountPercentage,
    DateTime ChangedAt,
    string? Reason);
