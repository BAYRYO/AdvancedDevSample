using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using Xunit;

namespace AdvancedDevSample.Test.Domain.Entities;

public class ProductTest
{
    [Fact]
    public void ChangePrice_Should_Update_Price_When_Product_Is_Active()
    {
        var product = new Product(10);

        product.ChangePrice(20);

        Assert.Equal(20, product.Price);
    }

    [Fact]
    public void ChangePrice_Should_Throw_Exception_When_Product_Is_Inactive()
    {
        var product = new Product(10);
        product.Deactivate();

        DomainException exception = Assert.Throws<DomainException>(() => product.ChangePrice(20));

        Assert.Equal("Produit inactif", exception.Message);
    }

    [Fact]
    public void ChangePrice_Should_Throw_Exception_When_Price_Is_Invalid()
    {
        var product = new Product(10);

        DomainException exception = Assert.Throws<DomainException>(() => product.ChangePrice(0));

        Assert.Equal("Prix invalide", exception.Message);
    }

    [Fact]
    public void ChangePrice_Should_Throw_Exception_When_Price_Is_Negative()
    {
        var product = new Product(10);

        DomainException exception = Assert.Throws<DomainException>(() => product.ChangePrice(-5));

        Assert.Equal("Prix invalide", exception.Message);
    }

    [Fact]
    public void ApplyDiscount_Should_Set_CurrentDiscount()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));

        product.ApplyDiscount(25m);

        Assert.NotNull(product.CurrentDiscount);
        Assert.Equal(25m, product.CurrentDiscount!.Value.Percentage);
    }

    [Fact]
    public void ApplyDiscount_Should_Throw_When_Product_Is_Inactive()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        product.Deactivate();

        DomainException exception = Assert.Throws<DomainException>(() => product.ApplyDiscount(25m));

        Assert.Equal("Impossible d'appliquer une reduction a un produit inactif.", exception.Message);
    }

    [Fact]
    public void ApplyDiscount_Should_Throw_When_Discount_Exceeds_Maximum()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));

        DomainException exception = Assert.Throws<DomainException>(() => product.ApplyDiscount(60m));

        Assert.Equal("La reduction ne peut pas depasser 50%.", exception.Message);
    }

    [Fact]
    public void RemoveDiscount_Should_Clear_CurrentDiscount()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        product.ApplyDiscount(25m);

        product.RemoveDiscount();

        Assert.Null(product.CurrentDiscount);
    }

    [Fact]
    public void GetEffectivePrice_Should_Return_Full_Price_When_No_Discount()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));

        decimal effectivePrice = product.GetEffectivePrice();

        Assert.Equal(100m, effectivePrice);
    }

    [Fact]
    public void GetEffectivePrice_Should_Return_Discounted_Price_When_Discount_Applied()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        product.ApplyDiscount(25m);

        decimal effectivePrice = product.GetEffectivePrice();

        Assert.Equal(75m, effectivePrice);
    }

    [Fact]
    public void AddStock_Should_Increase_Stock_Quantity()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"), stock: 10);

        product.AddStock(5);

        Assert.Equal(15, product.Stock.Quantity);
    }

    [Fact]
    public void RemoveStock_Should_Decrease_Stock_Quantity()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"), stock: 10);

        product.RemoveStock(5);

        Assert.Equal(5, product.Stock.Quantity);
    }

    [Fact]
    public void RemoveStock_Should_Throw_When_Insufficient_Stock()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"), stock: 10);

        DomainException exception = Assert.Throws<DomainException>(() => product.RemoveStock(15));

        Assert.Equal("Stock insuffisant pour cette operation.", exception.Message);
    }

    [Fact]
    public void UpdateName_Should_Update_Name()
    {
        var product = new Product("Old Name", 100m, new Sku("TEST-001"));

        product.UpdateName("New Name");

        Assert.Equal("New Name", product.Name);
    }

    [Fact]
    public void UpdateName_Should_Throw_When_Name_Is_Empty()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));

        DomainException exception = Assert.Throws<DomainException>(() => product.UpdateName(""));

        Assert.Equal("Le nom du produit est obligatoire.", exception.Message);
    }

    [Fact]
    public void UpdateName_Should_Throw_When_Name_Too_Long()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        string longName = new string('a', 201);

        DomainException exception = Assert.Throws<DomainException>(() => product.UpdateName(longName));

        Assert.Equal("Le nom du produit ne peut pas depasser 200 caracteres.", exception.Message);
    }

    [Fact]
    public void UpdateDescription_Should_Update_And_Trim_Description()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));

        product.UpdateDescription("  New description  ");

        Assert.Equal("New description", product.Description);
    }

    [Fact]
    public void UpdateDescription_Should_Set_Null_When_Description_Is_Null()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"), description: "Existing");

        product.UpdateDescription(null);

        Assert.Null(product.Description);
    }

    [Fact]
    public void UpdateDescription_Should_Throw_When_Description_Is_Too_Long()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        string longDescription = new string('a', Product.MaxDescriptionLength + 1);

        DomainException exception = Assert.Throws<DomainException>(() => product.UpdateDescription(longDescription));

        Assert.Equal($"La description ne peut pas depasser {Product.MaxDescriptionLength} caracteres.", exception.Message);
    }

    [Fact]
    public void UpdateCategory_Should_Update_CategoryId()
    {
        var product = new Product("Test Product", 100m, new Sku("TEST-001"));
        var categoryId = Guid.NewGuid();

        product.UpdateCategory(categoryId);

        Assert.Equal(categoryId, product.CategoryId);
    }

    [Fact]
    public void Activate_Should_Set_IsActive_True()
    {
        var product = new Product(10);
        product.Deactivate();

        product.Activate();

        Assert.True(product.IsActive);
    }

    [Fact]
    public void Deactivate_Should_Set_IsActive_False()
    {
        var product = new Product(10);

        product.Deactivate();

        Assert.False(product.IsActive);
    }

    [Fact]
    public void BackwardCompatibility_Constructor_With_Price_Only()
    {
        var product = new Product(10m);

        Assert.Equal(10m, product.Price);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void BackwardCompatibility_Constructor_With_Id_And_Price()
    {
        var id = Guid.NewGuid();
        var product = new Product(id, 10m);

        Assert.Equal(id, product.Id);
        Assert.Equal(10m, product.Price);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void BackwardCompatibility_Constructor_With_EmptyId_And_Price_GeneratesNewId()
    {
        var product = new Product(Guid.Empty, 10m);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(10m, product.Price);
        Assert.True(product.IsActive);
    }

    [Fact]
    public void BackwardCompatibility_Constructor_With_All_Parameters()
    {
        var id = Guid.NewGuid();
        var product = new Product(id, 10m, false);

        Assert.Equal(id, product.Id);
        Assert.Equal(10m, product.Price);
        Assert.False(product.IsActive);
    }

    [Fact]
    public void BackwardCompatibility_Constructor_With_EmptyId_And_All_Parameters_GeneratesNewId()
    {
        var product = new Product(Guid.Empty, 10m, false);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(10m, product.Price);
        Assert.False(product.IsActive);
    }

    [Fact]
    public void Reconstitution_Constructor_WithEmptyId_GeneratesNewId()
    {
        var product = new Product(new Product.ReconstitutionData
        {
            Id = Guid.Empty,
            Name = "Reconstituted",
            Price = 99m,
            Sku = new Sku("RECON-001"),
            Stock = 3,
            Description = "from persistence",
            CategoryId = null,
            DiscountPercentage = 10m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        });

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(89.1m, product.GetEffectivePrice());
    }

    [Fact]
    public void Reconstitution_Constructor_WithoutDiscount_Keeps_EffectivePrice_Equal_To_Price()
    {
        var explicitId = Guid.NewGuid();
        DateTime createdAt = DateTime.UtcNow.AddDays(-2);
        DateTime updatedAt = DateTime.UtcNow.AddDays(-1);

        var product = new Product(new Product.ReconstitutionData
        {
            Id = explicitId,
            Name = "No Discount",
            Price = 42m,
            Sku = new Sku("RECON-002"),
            Stock = 1,
            Description = null,
            CategoryId = null,
            DiscountPercentage = null,
            IsActive = true,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        });

        Assert.Equal(explicitId, product.Id);
        Assert.Null(product.CurrentDiscount);
        Assert.Equal(42m, product.GetEffectivePrice());
        Assert.Equal(createdAt, product.CreatedAt);
        Assert.Equal(updatedAt, product.UpdatedAt);
    }

    [Fact]
    public void Reconstitution_Constructor_WithNullName_DefaultsToEmptyString()
    {
        var product = new Product(new Product.ReconstitutionData
        {
            Id = Guid.NewGuid(),
            Name = null!,
            Price = 42m,
            Sku = new Sku("RECON-003"),
            Stock = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        });

        Assert.Equal(string.Empty, product.Name);
    }
}
