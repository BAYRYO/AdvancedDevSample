using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.ValueObjects;

/// <summary>
/// Value Object représentant une quantité de stock non-négative.
/// </summary>
public readonly record struct Stock
{
    public int Quantity { get; init; }

    public Stock(int quantity)
    {
        if (quantity < 0)
        {
            throw new DomainException("Le stock ne peut pas etre negatif.");
        }

        Quantity = quantity;
    }

    public Stock Add(int amount)
    {
        if (amount < 0)
        {
            throw new DomainException("Le stock ne peut pas etre negatif.");
        }

        return new Stock(Quantity + amount);
    }

    public Stock Remove(int amount)
    {
        if (amount < 0)
        {
            throw new DomainException("Le stock ne peut pas etre negatif.");
        }

        if (amount > Quantity)
        {
            throw new DomainException("Stock insuffisant pour cette operation.");
        }

        return new Stock(Quantity - amount);
    }

    public override string ToString() => Quantity.ToString();

    public static implicit operator int(Stock stock) => stock.Quantity;
}
