using System.Net;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.ValueObjects;
using AdvancedDevSample.Test.API.Integration;

namespace AdvancedDevSample.Test.Application;

public class ProductServiceTests
{
    private readonly InMemoryProductRepository _productRepository = new();
    private readonly InMemoryCategoryRepository _categoryRepository = new();
    private readonly InMemoryPriceHistoryRepository _priceHistoryRepository = new();
    private readonly FakeTransactionManager _transactionManager = new();

    private ProductService CreateService()
    {
        return new ProductService(
            _productRepository,
            _categoryRepository,
            _priceHistoryRepository,
            _transactionManager);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateSku_ThrowsDuplicateSkuException()
    {
        ProductService service = CreateService();
        _productRepository.Seed(new Product("Existing", 10m, new Sku("DUPL-001")));

        var request = new CreateProductRequest(
            Name: "Duplicate",
            Sku: "DUPL-001",
            Price: 20m);

        await Assert.ThrowsAsync<DuplicateSkuException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithUnknownCategory_ThrowsCategoryNotFoundException()
    {
        ProductService service = CreateService();

        var request = new CreateProductRequest(
            Name: "New",
            Sku: "CAT-404",
            Price: 20m,
            CategoryId: Guid.NewGuid());

        await Assert.ThrowsAsync<CategoryNotFoundException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task SearchAsync_ClampsInvalidPagingValues()
    {
        ProductService service = CreateService();
        _productRepository.Seed(new Product("Alpha", 10m, new Sku("A-001")));
        _productRepository.Seed(new Product("Beta", 20m, new Sku("B-001")));

        PagedResponse<ProductResponse> response = await service.SearchAsync(new ProductSearchRequest { Page = 0, PageSize = 500 });

        Assert.Equal(1, response.Page);
        Assert.Equal(100, response.PageSize);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.TotalPages);
        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task UpdateAsync_WithPriceStockCategoryAndActivation_UpdatesAllFields()
    {
        ProductService service = CreateService();
        var oldCategory = new Category("Old", "Old category");
        var newCategory = new Category("New", "New category");
        await _categoryRepository.SaveAsync(oldCategory);
        await _categoryRepository.SaveAsync(newCategory);

        var product = new Product(
            name: "Phone",
            price: 100m,
            sku: new Sku("PHN-001"),
            stock: 10,
            description: "Old desc",
            categoryId: oldCategory.Id);
        _productRepository.Seed(product);

        ProductResponse updated = await service.UpdateAsync(product.Id, new UpdateProductRequest(
            Name: "Phone 2",
            Description: "New desc",
            Price: 120m,
            Stock: 7,
            CategoryId: newCategory.Id,
            IsActive: false));

        Assert.Equal("Phone 2", updated.Name);
        Assert.Equal("New desc", updated.Description);
        Assert.Equal(120m, updated.Price);
        Assert.Equal(7, updated.Stock);
        Assert.Equal(newCategory.Id, updated.CategoryId);
        Assert.False(updated.IsActive);

        IReadOnlyList<PriceHistory> history = await _priceHistoryRepository.GetByProductIdAsync(product.Id);
        Assert.Single(history);
        Assert.Equal(100m, history[0].OldPrice);
        Assert.Equal(120m, history[0].NewPrice);
    }

    [Fact]
    public async Task UpdateAsync_WithClearCategory_RemovesCategory()
    {
        ProductService service = CreateService();
        var category = new Category("Tech", "Tech category");
        await _categoryRepository.SaveAsync(category);

        var product = new Product("Laptop", 999m, new Sku("LAP-001"), categoryId: category.Id);
        _productRepository.Seed(product);

        ProductResponse updated = await service.UpdateAsync(product.Id, new UpdateProductRequest(ClearCategory: true));

        Assert.Null(updated.CategoryId);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownCategory_ThrowsCategoryNotFoundException()
    {
        ProductService service = CreateService();
        var product = new Product("Laptop", 999m, new Sku("LAP-002"));
        _productRepository.Seed(product);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            service.UpdateAsync(product.Id, new UpdateProductRequest(CategoryId: Guid.NewGuid())));
    }

    [Fact]
    public async Task RemoveDiscountAsync_WithoutDiscount_ReturnsProductAndDoesNotWriteHistory()
    {
        ProductService service = CreateService();
        var product = new Product("No Discount", 50m, new Sku("NO-DISC-1"));
        _productRepository.Seed(product);

        ProductResponse response = await service.RemoveDiscountAsync(product.Id);

        Assert.Equal(50m, response.EffectivePrice);
        Assert.Null(response.DiscountPercentage);

        IReadOnlyList<PriceHistory> history = await _priceHistoryRepository.GetByProductIdAsync(product.Id);
        Assert.Empty(history);
    }

    [Fact]
    public async Task ChangePriceAsync_WithUnknownProduct_ThrowsApplicationServiceExceptionNotFound()
    {
        ProductService service = CreateService();

        ApplicationServiceException exception = await Assert.ThrowsAsync<ApplicationServiceException>(() =>
            service.ChangePriceAsync(Guid.NewGuid(), new ChangePriceRequest(10m)));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownProduct_ThrowsProductNotFoundException()
    {
        ProductService service = CreateService();

        await Assert.ThrowsAsync<ProductNotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    private sealed class FakeTransactionManager : ITransactionManager
    {
        public Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default) => action();

        public Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default) => action();
    }
}
