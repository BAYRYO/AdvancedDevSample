using AdvancedDevSample.Domain.Entities;
using Bogus;

namespace AdvancedDevSample.Test.Factories;

public class PriceHistoryFactory
{
    private readonly Faker _faker;
    private Guid? _id;
    private Guid? _productId;
    private decimal? _oldPrice;
    private decimal? _newPrice;
    private decimal? _discountPercentage;
    private DateTime? _changedAt;
    private string? _reason;

    public PriceHistoryFactory()
    {
        _faker = new Faker("fr");
    }

    public static PriceHistoryFactory Create() => new();

    public PriceHistoryFactory WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public PriceHistoryFactory ForProduct(Guid productId)
    {
        _productId = productId;
        return this;
    }

    public PriceHistoryFactory WithOldPrice(decimal oldPrice)
    {
        _oldPrice = oldPrice;
        return this;
    }

    public PriceHistoryFactory WithNewPrice(decimal newPrice)
    {
        _newPrice = newPrice;
        return this;
    }

    public PriceHistoryFactory WithDiscount(decimal percentage)
    {
        _discountPercentage = percentage;
        return this;
    }

    public PriceHistoryFactory WithChangedAt(DateTime changedAt)
    {
        _changedAt = changedAt;
        return this;
    }

    public PriceHistoryFactory WithReason(string? reason)
    {
        _reason = reason;
        return this;
    }

    public PriceHistory Build()
    {
        Guid id = _id ?? Guid.NewGuid();
        Guid productId = _productId ?? Guid.NewGuid();
        decimal oldPrice = _oldPrice ?? decimal.Parse(_faker.Commerce.Price(10, 1000));
        decimal newPrice = _newPrice ?? decimal.Parse(_faker.Commerce.Price(10, 1000));
        DateTime changedAt = _changedAt ?? _faker.Date.Past(1);
        string? reason = _reason ?? _faker.PickRandom(PriceChangeReasons);

        return new PriceHistory(
            id,
            productId,
            oldPrice,
            newPrice,
            _discountPercentage,
            changedAt,
            reason
        );
    }

    public List<PriceHistory> BuildMany(int count, Guid? productId = null)
    {
        return
        [
            .. Enumerable.Range(0, count)
            .Select(_ => new PriceHistoryFactory()
                .ForProduct(productId ?? _productId ?? Guid.NewGuid())
                .Build())
        ];
    }

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

    public static Faker<PriceHistory> Faker(Guid? productId = null)
    {
        return new Faker<PriceHistory>("fr")
            .CustomInstantiator(f =>
            {
                decimal oldPrice = decimal.Parse(f.Commerce.Price(10, 1000));
                decimal priceChange = f.Random.Decimal(-0.3m, 0.3m);
                decimal newPrice = Math.Max(1, oldPrice * (1 + priceChange));

                return new PriceHistory(
                    Guid.NewGuid(),
                    productId ?? Guid.NewGuid(),
                    oldPrice,
                    Math.Round(newPrice, 2),
                    f.Random.Bool(0.2f) ? f.Random.Decimal(5, 30) : null,
                    f.Date.Past(1),
                    f.PickRandom(PriceChangeReasons)
                );
            });
    }
}
