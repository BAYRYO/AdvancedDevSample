using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Infrastructure.Persistence.Entities;

namespace AdvancedDevSample.Infrastructure.Persistence.Mappers;

public static class PriceHistoryMapper
{
    public static PriceHistory ToDomain(PriceHistoryEntity entity)
    {
        return new PriceHistory(
            id: entity.Id,
            productId: entity.ProductId,
            oldPrice: entity.OldPrice,
            newPrice: entity.NewPrice,
            discountPercentage: entity.DiscountPercentage,
            changedAt: entity.ChangedAt,
            reason: entity.Reason);
    }

    public static PriceHistoryEntity ToEntity(PriceHistory priceHistory)
    {
        return new PriceHistoryEntity
        {
            Id = priceHistory.Id,
            ProductId = priceHistory.ProductId,
            OldPrice = priceHistory.OldPrice,
            NewPrice = priceHistory.NewPrice,
            DiscountPercentage = priceHistory.DiscountPercentage,
            ChangedAt = priceHistory.ChangedAt,
            Reason = priceHistory.Reason
        };
    }
}
