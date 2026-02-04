namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Entité immuable représentant un historique de changement de prix.
/// </summary>
public class PriceHistory
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal OldPrice { get; private set; }
    public decimal NewPrice { get; private set; }
    public decimal? DiscountPercentage { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public string? Reason { get; private set; }

    public PriceHistory(
        Guid productId,
        decimal oldPrice,
        decimal newPrice,
        decimal? discountPercentage = null,
        string? reason = null)
        : this(Guid.NewGuid(), productId, oldPrice, newPrice, discountPercentage, DateTime.UtcNow, reason)
    {
    }

    public PriceHistory(
        Guid id,
        Guid productId,
        decimal oldPrice,
        decimal newPrice,
        decimal? discountPercentage,
        DateTime changedAt,
        string? reason)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
        DiscountPercentage = discountPercentage;
        ChangedAt = changedAt;
        Reason = reason;
    }
}
