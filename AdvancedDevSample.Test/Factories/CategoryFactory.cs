using AdvancedDevSample.Domain.Entities;
using Bogus;

namespace AdvancedDevSample.Test.Factories;

public class CategoryFactory
{
    private readonly Faker _faker;
    private Guid? _id;
    private string? _name;
    private string? _description;
    private bool _isActive = true;
    private DateTime? _createdAt;
    private DateTime? _updatedAt;

    public CategoryFactory()
    {
        _faker = new Faker("fr");
    }

    public static CategoryFactory Create() => new();

    public CategoryFactory WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CategoryFactory WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryFactory WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public CategoryFactory AsActive()
    {
        _isActive = true;
        return this;
    }

    public CategoryFactory AsInactive()
    {
        _isActive = false;
        return this;
    }

    public CategoryFactory WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public CategoryFactory WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Category Build()
    {
        Guid id = _id ?? Guid.NewGuid();
        string name = _name ?? _faker.Commerce.Categories(1)[0];
        string? description = _description ?? _faker.Lorem.Sentence();
        DateTime createdAt = _createdAt ?? DateTime.UtcNow;
        DateTime updatedAt = _updatedAt ?? DateTime.UtcNow;

        return new Category(id, name, description, _isActive, createdAt, updatedAt);
    }

    public static List<Category> BuildMany(int count) =>
    [
        .. Enumerable.Range(0, count)
            .Select(_ => new CategoryFactory().Build())
    ];

    public static Faker<Category> Faker() => new Faker<Category>("fr")
        .CustomInstantiator(f => new Category(
            Guid.NewGuid(),
            f.Commerce.Categories(1)[0],
            f.Lorem.Sentence(),
            f.Random.Bool(0.9f),
            f.Date.Past(1),
            DateTime.UtcNow
        ));
}
