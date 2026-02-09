using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using Xunit;

namespace AdvancedDevSample.Test.Domain.ValueObjects;

public class PriceTests
{
    [Fact]
    public void Constructor_Should_Create_Price_When_Value_Is_Valid()
    {
        var price = new Price(10.50m);
        Assert.Equal(10.50m, price.Value);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Is_Zero()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Price(0));
        Assert.Equal("Un prix doit etre strictement positif.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Is_Negative()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Price(-10));
        Assert.Equal("Un prix doit etre strictement positif.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Below_MinValue()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Price(0.001m));
        Assert.Equal("Le prix minimum est de 0,01 EUR.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Above_MaxValue()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Price(1000000m));
        Assert.Equal("Le prix maximum est de 999 999,99 EUR.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Accept_MinValue()
    {
        var price = new Price(Price.MinValue);
        Assert.Equal(0.01m, price.Value);
    }

    [Fact]
    public void Constructor_Should_Accept_MaxValue()
    {
        var price = new Price(Price.MaxValue);
        Assert.Equal(999999.99m, price.Value);
    }

    [Fact]
    public void ApplyDiscount_Should_Return_Discounted_Price()
    {
        var price = new Price(100m);
        var discount = new Discount(25m);

        Price discountedPrice = price.ApplyDiscount(discount);

        Assert.Equal(75m, discountedPrice.Value);
    }

    [Fact]
    public void ImplicitConversion_Should_Return_Value()
    {
        var price = new Price(50m);
        decimal value = price;
        Assert.Equal(50m, value);
    }

    [Fact]
    public void ToString_Should_Return_Formatted_Value()
    {
        var price = new Price(10.5m);
        Assert.Equal("10.50", price.ToString());
    }
}
