namespace AdvancedDevSample.Infrastructure.Persistence.Entities;

public class PriceHistoryEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? Reason { get; set; }

    public ProductEntity? Product { get; set; }
}
