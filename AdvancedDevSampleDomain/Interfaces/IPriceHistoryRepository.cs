using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public interface IPriceHistoryRepository
{
    Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(Guid productId);
    Task SaveAsync(PriceHistory priceHistory);
}
