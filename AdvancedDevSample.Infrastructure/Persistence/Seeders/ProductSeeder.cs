using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public class ProductSeeder : ISeeder
{
    public int Order => 2;

    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Products.AnyAsync(cancellationToken))
        {
            return;
        }

        var categoryIds = await context.Categories
            .Where(c => c.IsActive)
            .Select(c => (Guid?)c.Id)
            .ToListAsync(cancellationToken);

        if (categoryIds.Count == 0)
        {
            categoryIds.Add(null);
        }

        var products = GetPredefinedProducts(categoryIds)
            .Concat(GenerateRandomProducts(20, categoryIds))
            .ToList();

        await context.Products.AddRangeAsync(products, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<ProductEntity> GetPredefinedProducts(List<Guid?> categoryIds)
    {
        var now = DateTime.UtcNow;
        Guid? electronicsId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        Guid? clothingId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        Guid? foodId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        return new List<ProductEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "iPhone 15 Pro",
                Description = "Smartphone Apple derniere generation avec puce A17 Pro",
                Price = 1199.99m,
                IsActive = true,
                Sku = "IPHONE-15PRO",
                Stock = 50,
                CategoryId = categoryIds.Contains(electronicsId) ? electronicsId : categoryIds.FirstOrDefault(),
                DiscountPercentage = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "MacBook Air M3",
                Description = "Ordinateur portable ultra-leger avec puce Apple M3",
                Price = 1299.99m,
                IsActive = true,
                Sku = "MACBOOK-M3",
                Stock = 30,
                CategoryId = categoryIds.Contains(electronicsId) ? electronicsId : categoryIds.FirstOrDefault(),
                DiscountPercentage = 10m,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "T-Shirt Premium",
                Description = "T-shirt en coton bio de haute qualite",
                Price = 29.99m,
                IsActive = true,
                Sku = "TSHIRT-PREM",
                Stock = 200,
                CategoryId = categoryIds.Contains(clothingId) ? clothingId : categoryIds.FirstOrDefault(),
                DiscountPercentage = null,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Cafe Premium Arabica",
                Description = "Cafe en grains 100% Arabica torrefaction artisanale",
                Price = 14.99m,
                IsActive = true,
                Sku = "CAFE-ARABICA",
                Stock = 100,
                CategoryId = categoryIds.Contains(foodId) ? foodId : categoryIds.FirstOrDefault(),
                DiscountPercentage = 5m,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Produit Desactive",
                Description = "Ce produit n'est plus disponible",
                Price = 99.99m,
                IsActive = false,
                Sku = "PROD-INACTIF",
                Stock = 0,
                CategoryId = categoryIds.FirstOrDefault(),
                DiscountPercentage = null,
                CreatedAt = now.AddMonths(-6),
                UpdatedAt = now.AddMonths(-3)
            }
        };
    }

    private static IEnumerable<ProductEntity> GenerateRandomProducts(int count, List<Guid?> categoryIds)
    {
        var skuIndex = 1000;

        var faker = new Faker<ProductEntity>("fr")
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Commerce.ProductName())
            .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
            .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(5, 500), 2))
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(p => p.Sku, _ => $"SKU-{Interlocked.Increment(ref skuIndex):D6}")
            .RuleFor(p => p.Stock, f => f.Random.Int(0, 150))
            .RuleFor(p => p.CategoryId, f => f.PickRandom(categoryIds))
            .RuleFor(p => p.DiscountPercentage, f => f.Random.Bool(0.2f) ? Math.Round(f.Random.Decimal(5, 30), 2) : null)
            .RuleFor(p => p.CreatedAt, f => f.Date.Past(1))
            .RuleFor(p => p.UpdatedAt, _ => DateTime.UtcNow);

        return faker.Generate(count);
    }
}
