using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.ValueObjects;
using Bogus;

namespace AdvancedDevSample.Test.Factories;

public class ProductFactory
{
    private readonly Faker _faker;
    private Guid? _id;
    private string? _name;
    private string? _description;
    private decimal? _price;
    private string? _sku;
    private int? _stock;
    private Guid? _categoryId;
    private decimal? _discountPercentage;
    private bool _isActive = true;
    private DateTime? _createdAt;
    private DateTime? _updatedAt;

    private static int _skuCounter;

    public ProductFactory()
    {
        _faker = new Faker("fr");
    }

    public static ProductFactory Create() => new();

    public ProductFactory WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public ProductFactory WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductFactory WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ProductFactory WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ProductFactory WithSku(string sku)
    {
        _sku = sku;
        return this;
    }

    public ProductFactory WithStock(int stock)
    {
        _stock = stock;
        return this;
    }

    public ProductFactory WithCategory(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    public ProductFactory WithDiscount(decimal percentage)
    {
        _discountPercentage = percentage;
        return this;
    }

    public ProductFactory AsActive()
    {
        _isActive = true;
        return this;
    }

    public ProductFactory AsInactive()
    {
        _isActive = false;
        return this;
    }

    public ProductFactory WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public ProductFactory WithUpdatedAt(DateTime updatedAt)
    {
        _updatedAt = updatedAt;
        return this;
    }

    public Product Build()
    {
        var id = _id ?? Guid.NewGuid();
        var name = _name ?? _faker.Commerce.ProductName();
        var description = _description ?? _faker.Commerce.ProductDescription();
        var price = _price ?? decimal.Parse(_faker.Commerce.Price(10, 1000));
        var sku = _sku ?? GenerateUniqueSku();
        var stock = _stock ?? _faker.Random.Int(0, 100);
        var createdAt = _createdAt ?? DateTime.UtcNow;
        var updatedAt = _updatedAt ?? DateTime.UtcNow;

        return new Product(new Product.ReconstitutionData
        {
            Id = id,
            Name = name,
            Price = price,
            Sku = new Sku(sku),
            Stock = stock,
            Description = description,
            CategoryId = _categoryId,
            DiscountPercentage = _discountPercentage,
            IsActive = _isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        });
    }

    public List<Product> BuildMany(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new ProductFactory()
                .WithCategory(_categoryId)
                .Build())
            .ToList();
    }

    private static string GenerateUniqueSku()
    {
        var counter = Interlocked.Increment(ref _skuCounter);
        return $"SKU-{counter:D6}";
    }

    public static Faker<Product> Faker(Guid? categoryId = null)
    {
        var skuIndex = 0;
        return new Faker<Product>("fr")
            .CustomInstantiator(f =>
            {
                var sku = $"SKU-{Interlocked.Increment(ref skuIndex):D6}";
                return new Product(new Product.ReconstitutionData
                {
                    Id = Guid.NewGuid(),
                    Name = f.Commerce.ProductName(),
                    Price = decimal.Parse(f.Commerce.Price(10, 1000)),
                    Sku = new Sku(sku),
                    Stock = f.Random.Int(0, 100),
                    Description = f.Commerce.ProductDescription(),
                    CategoryId = categoryId,
                    DiscountPercentage = f.Random.Bool(0.2f) ? f.Random.Decimal(5, 30) : null,
                    IsActive = f.Random.Bool(0.9f),
                    CreatedAt = f.Date.Past(1),
                    UpdatedAt = DateTime.UtcNow
                });
            });
    }
}
