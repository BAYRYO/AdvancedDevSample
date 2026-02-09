using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using Xunit;

namespace AdvancedDevSample.Test.Domain.ValueObjects;

public class DiscountTests
{
    [Fact]
    public void Constructor_Should_Create_Discount_When_Value_Is_Valid()
    {
        var discount = new Discount(25m);
        Assert.Equal(25m, discount.Percentage);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero()
    {
        var discount = new Discount(0m);
        Assert.Equal(0m, discount.Percentage);
    }

    [Fact]
    public void Constructor_Should_Accept_Maximum()
    {
        var discount = new Discount(50m);
        Assert.Equal(50m, discount.Percentage);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Negative()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Discount(-10m));
        Assert.Equal("La reduction ne peut pas etre negative.", exception.Message);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Above_Maximum()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Discount(51m));
        Assert.Equal("La reduction ne peut pas depasser 50%.", exception.Message);
    }

    [Fact]
    public void ApplyTo_Should_Calculate_Correct_Discounted_Price()
    {
        var discount = new Discount(25m);
        decimal result = discount.ApplyTo(100m);
        Assert.Equal(75m, result);
    }

    [Fact]
    public void ApplyTo_Should_Handle_Zero_Discount()
    {
        var discount = new Discount(0m);
        decimal result = discount.ApplyTo(100m);
        Assert.Equal(100m, result);
    }

    [Fact]
    public void ApplyTo_Should_Handle_Max_Discount()
    {
        var discount = new Discount(50m);
        decimal result = discount.ApplyTo(100m);
        Assert.Equal(50m, result);
    }

    [Fact]
    public void ApplyTo_Should_Round_To_Two_Decimals()
    {
        var discount = new Discount(33m);
        decimal result = discount.ApplyTo(100m);
        Assert.Equal(67m, result);
    }

    [Fact]
    public void ImplicitConversion_Should_Return_Percentage()
    {
        var discount = new Discount(25m);
        decimal value = discount;
        Assert.Equal(25m, value);
    }

    [Fact]
    public void ToString_Should_Return_Formatted_Percentage()
    {
        var discount = new Discount(25m);
        Assert.Equal("25%", discount.ToString());
    }
}
