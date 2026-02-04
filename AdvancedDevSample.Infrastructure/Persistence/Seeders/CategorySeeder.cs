using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public class CategorySeeder : ISeeder
{
    public int Order => 1;

    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = GetPredefinedCategories()
            .Concat(GenerateRandomCategories(5))
            .ToList();

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<CategoryEntity> GetPredefinedCategories()
    {
        var now = DateTime.UtcNow;

        return new List<CategoryEntity>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Electronique",
                Description = "Appareils electroniques et gadgets",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Vetements",
                Description = "Mode et habillement",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Alimentation",
                Description = "Produits alimentaires et boissons",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Maison",
                Description = "Decoration et amenagement interieur",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Sport",
                Description = "Equipements et accessoires sportifs",
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        };
    }

    private static IEnumerable<CategoryEntity> GenerateRandomCategories(int count)
    {
        var faker = new Faker<CategoryEntity>("fr")
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1))
            .RuleFor(c => c.UpdatedAt, _ => DateTime.UtcNow);

        return faker.Generate(count);
    }
}
