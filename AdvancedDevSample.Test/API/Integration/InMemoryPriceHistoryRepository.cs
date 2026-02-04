using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryPriceHistoryRepository : IPriceHistoryRepository
{
    private readonly List<PriceHistory> _store = new();

    public Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(Guid productId)
    {
        IReadOnlyList<PriceHistory> result = _store
            .Where(ph => ph.ProductId == productId)
            .OrderByDescending(ph => ph.ChangedAt)
            .ToList();
        return Task.FromResult(result);
    }

    public Task SaveAsync(PriceHistory priceHistory)
    {
        _store.Add(priceHistory);
        return Task.CompletedTask;
    }

    public void Clear()
    {
        _store.Clear();
    }
}
