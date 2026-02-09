using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public class PriceHistorySeeder : ISeeder
{
    public int Order => 3;

    private static readonly string[] PriceChangeReasons =
    [
        "Ajustement saisonnier",
        "Promotion speciale",
        "Nouveau fournisseur",
        "Augmentation des couts",
        "Alignement concurrentiel",
        "Soldes",
        "Black Friday",
        "Fin de serie",
        "Nouveau tarif"
    ];

    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.PriceHistories.AnyAsync(cancellationToken))
        {
            return;
        }

        var products = await context.Products
            .Select(p => new { p.Id, p.Price })
            .ToListAsync(cancellationToken);

        if (products.Count == 0)
        {
            return;
        }

        var priceHistories = new List<PriceHistoryEntity>();

        foreach (var product in products)
        {
            int historyCount = Random.Shared.Next(1, 5);
            decimal currentPrice = product.Price;

            for (int i = 0; i < historyCount; i++)
            {
                decimal priceChange = (decimal)(Random.Shared.NextDouble() * 0.4 - 0.2);
                decimal oldPrice = Math.Max(1, Math.Round(currentPrice * (1 + priceChange), 2));

                priceHistories.Add(new PriceHistoryEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    OldPrice = oldPrice,
                    NewPrice = currentPrice,
                    DiscountPercentage = Random.Shared.Next(0, 5) == 0
                        ? Math.Round((decimal)(Random.Shared.NextDouble() * 25 + 5), 2)
                        : null,
                    ChangedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 365)),
                    Reason = PriceChangeReasons[Random.Shared.Next(PriceChangeReasons.Length)]
                });

                currentPrice = oldPrice;
            }
        }

        await context.PriceHistories.AddRangeAsync(priceHistories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
