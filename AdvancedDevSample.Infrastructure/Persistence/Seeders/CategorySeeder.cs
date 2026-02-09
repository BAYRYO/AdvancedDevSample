using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Persistence.Seeders;

public class CategorySeeder : ISeeder
{
    private const int RandomCategoryCount = 5;

    private static readonly (Guid Id, string Name, string Description)[] PredefinedCategoryData =
    [
        (
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "Electronique",
            "Appareils electroniques et gadgets"
        ),
        (
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Vetements",
            "Mode et habillement"
        ),
        (
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Alimentation",
            "Produits alimentaires et boissons"
        ),
        (
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Maison",
            "Decoration et amenagement interieur"
        ),
        (
            Guid.Parse("55555555-5555-5555-5555-555555555555"),
            "Sport",
            "Equipements et accessoires sportifs"
        )
    ];

    public int Order => 1;

    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Categories.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = GetPredefinedCategories()
            .Concat(GenerateRandomCategories(RandomCategoryCount))
            .ToList();

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<CategoryEntity> GetPredefinedCategories()
    {
        DateTime now = DateTime.UtcNow;

        return PredefinedCategoryData.Select(category => new CategoryEntity
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private static List<CategoryEntity> GenerateRandomCategories(int count)
    {
        Faker<CategoryEntity> faker = new Faker<CategoryEntity>("fr")
            .RuleFor(c => c.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(c => c.Description, f => f.Lorem.Sentence())
            .RuleFor(c => c.IsActive, f => f.Random.Bool(0.9f))
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1))
            .RuleFor(c => c.UpdatedAt, _ => DateTime.UtcNow);

        return faker.Generate(count);
    }
}
