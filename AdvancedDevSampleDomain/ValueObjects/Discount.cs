using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une réduction de prix (0-50% maximum).
/// </summary>
public readonly record struct Discount
{
    public const decimal MaxPercentage = 50m;

    public decimal Percentage { get; init; }

    public Discount(decimal percentage)
    {
        if (percentage < 0m)
        {
            throw new DomainException("La reduction ne peut pas etre negative.");
        }

        if (percentage > MaxPercentage)
        {
            throw new DomainException("La reduction ne peut pas depasser 50%.");
        }

        Percentage = percentage;
    }

    public decimal ApplyTo(decimal price)
    {
        return Math.Round(price * (1m - Percentage / 100m), 2);
    }

    public override string ToString() => $"{Percentage}%";

    public static implicit operator decimal(Discount discount) => discount.Percentage;
}
