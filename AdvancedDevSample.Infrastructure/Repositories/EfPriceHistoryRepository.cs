using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfPriceHistoryRepository : IPriceHistoryRepository
{
    private readonly AppDbContext _context;

    public EfPriceHistoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PriceHistory>> GetByProductIdAsync(Guid productId)
    {
        var entities = await _context.PriceHistories
            .Where(ph => ph.ProductId == productId)
            .OrderByDescending(ph => ph.ChangedAt)
            .ToListAsync();
        return entities.Select(PriceHistoryMapper.ToDomain).ToList();
    }

    public async Task SaveAsync(PriceHistory priceHistory)
    {
        var entity = PriceHistoryMapper.ToEntity(priceHistory);
        _context.PriceHistories.Add(entity);
        await _context.SaveChangesAsync();
    }
}
