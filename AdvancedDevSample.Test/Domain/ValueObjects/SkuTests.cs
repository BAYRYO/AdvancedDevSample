using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using Xunit;

namespace AdvancedDevSample.Test.Domain.ValueObjects;

public class SkuTests
{
    [Fact]
    public void Constructor_Should_Create_Sku_When_Value_Is_Valid()
    {
        var sku = new Sku("ABC-123");
        Assert.Equal("ABC-123", sku.Value);
    }

    [Fact]
    public void Constructor_Should_Normalize_To_Uppercase()
    {
        var sku = new Sku("abc-123");
        Assert.Equal("ABC-123", sku.Value);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Is_Empty()
    {
        var exception = Assert.Throws<DomainException>(() => new Sku(""));
        Assert.Equal("Le SKU ne peut pas etre vide.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Is_Whitespace()
    {
        var exception = Assert.Throws<DomainException>(() => new Sku("   "));
        Assert.Equal("Le SKU ne peut pas etre vide.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Too_Short()
    {
        var exception = Assert.Throws<DomainException>(() => new Sku("AB"));
        Assert.Equal("Le SKU doit contenir entre 3 et 20 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Too_Long()
    {
        var exception = Assert.Throws<DomainException>(() => new Sku("ABCDEFGHIJ1234567890X"));
        Assert.Equal("Le SKU doit contenir entre 3 et 20 caracteres.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Contains_Invalid_Characters()
    {
        var exception = Assert.Throws<DomainException>(() => new Sku("ABC_123"));
        Assert.Equal("Le SKU ne peut contenir que des lettres, chiffres et tirets.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Accept_Minimum_Length()
    {
        var sku = new Sku("ABC");
        Assert.Equal("ABC", sku.Value);
    }

    [Fact]
    public void Constructor_Should_Accept_Maximum_Length()
    {
        var sku = new Sku("ABCDEFGHIJ1234567890");
        Assert.Equal("ABCDEFGHIJ1234567890", sku.Value);
    }

    [Fact]
    public void ImplicitConversion_Should_Return_Value()
    {
        var sku = new Sku("TEST-SKU");
        string value = sku;
        Assert.Equal("TEST-SKU", value);
    }
}
