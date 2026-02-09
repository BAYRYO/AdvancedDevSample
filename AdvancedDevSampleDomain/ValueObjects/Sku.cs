using System.Text.RegularExpressions;
using AdvancedDevSample.Domain.Exceptions;

namespace AdvancedDevSample.Domain.ValueObjects;

/// <summary>
/// Value Object représentant un SKU (Stock Keeping Unit).
/// Format: 3-20 caractères alphanumériques avec tirets, auto-normalisé en majuscules.
/// </summary>
public readonly record struct Sku
{
    private static readonly Regex ValidPattern = new(
        @"^[A-Z0-9\-]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public string Value { get; init; }

    public Sku(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Le SKU ne peut pas etre vide.");
        }

        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length < 3 || normalized.Length > 20)
        {
            throw new DomainException("Le SKU doit contenir entre 3 et 20 caracteres.");
        }

        if (!ValidPattern.IsMatch(normalized))
        {
            throw new DomainException("Le SKU ne peut contenir que des lettres, chiffres et tirets.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;

    public static implicit operator string(Sku sku) => sku.Value;
}
