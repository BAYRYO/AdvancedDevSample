using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.ValueObjects
{
    /// <summary>
    /// Value Object représentant un prix strictement positif.
    /// Limites: 0.01 EUR - 999999.99 EUR
    /// </summary>
    public readonly record struct Price
    {
        public const decimal MinValue = 0.01m;
        public const decimal MaxValue = 999999.99m;

        public decimal Value { get; init; }

        public Price(decimal value)
        {
            if (value <= 0m)
            {
                throw new DomainException("Un prix doit etre strictement positif.");
            }

            if (value < MinValue)
            {
                throw new DomainException("Le prix minimum est de 0,01 EUR.");
            }

            if (value > MaxValue)
            {
                throw new DomainException("Le prix maximum est de 999 999,99 EUR.");
            }

            Value = value;
        }

        public Price ApplyDiscount(Discount discount)
        {
            decimal discountedValue = discount.ApplyTo(Value);
            return new Price(discountedValue);
        }

        public override string ToString() => Value.ToString("F2");

        public static implicit operator decimal(Price price) => price.Value;
    }
}
