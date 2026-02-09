using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using Xunit;

namespace AdvancedDevSample.Test.Domain.ValueObjects;

public class StockTests
{
    [Fact]
    public void Constructor_Should_Create_Stock_When_Value_Is_Valid()
    {
        var stock = new Stock(100);
        Assert.Equal(100, stock.Quantity);
    }

    [Fact]
    public void Constructor_Should_Accept_Zero()
    {
        var stock = new Stock(0);
        Assert.Equal(0, stock.Quantity);
    }

    [Fact]
    public void Constructor_Should_Throw_When_Value_Is_Negative()
    {
        DomainException exception = Assert.Throws<DomainException>(() => new Stock(-1));
        Assert.Equal("Le stock ne peut pas etre negatif.", exception.Message);
    }

    [Fact]
    public void Add_Should_Increase_Quantity()
    {
        var stock = new Stock(10);
        Stock result = stock.Add(5);
        Assert.Equal(15, result.Quantity);
    }

    [Fact]
    public void Add_Should_Throw_When_Amount_Is_Negative()
    {
        var stock = new Stock(10);
        DomainException exception = Assert.Throws<DomainException>(() => stock.Add(-5));
        Assert.Equal("Le stock ne peut pas etre negatif.", exception.Message);
    }

    [Fact]
    public void Remove_Should_Decrease_Quantity()
    {
        var stock = new Stock(10);
        Stock result = stock.Remove(5);
        Assert.Equal(5, result.Quantity);
    }

    [Fact]
    public void Remove_Should_Throw_When_Amount_Is_Negative()
    {
        var stock = new Stock(10);
        DomainException exception = Assert.Throws<DomainException>(() => stock.Remove(-5));
        Assert.Equal("Le stock ne peut pas etre negatif.", exception.Message);
    }

    [Fact]
    public void Remove_Should_Throw_When_Insufficient_Stock()
    {
        var stock = new Stock(10);
        DomainException exception = Assert.Throws<DomainException>(() => stock.Remove(15));
        Assert.Equal("Stock insuffisant pour cette operation.", exception.Message);
    }

    [Fact]
    public void Remove_Should_Allow_Removing_Exact_Quantity()
    {
        var stock = new Stock(10);
        Stock result = stock.Remove(10);
        Assert.Equal(0, result.Quantity);
    }

    [Fact]
    public void ImplicitConversion_Should_Return_Quantity()
    {
        var stock = new Stock(50);
        int value = stock;
        Assert.Equal(50, value);
    }
}
