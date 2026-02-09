using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.ValueObjects;

namespace AdvancedDevSample.Test.API.Integration;

public class ProductControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;
    private readonly HttpClient _adminClient;
    private readonly InMemoryProductRepository _repo;
    private readonly InMemoryCategoryRepository _categoryRepo;
    private readonly InMemoryPriceHistoryRepository _priceHistoryRepo;

    public ProductControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authenticatedClient = factory.CreateAuthenticatedClient(UserRole.User);
        _adminClient = factory.CreateAuthenticatedClient(UserRole.Admin);
        _repo = factory.ProductRepository;
        _categoryRepo = factory.CategoryRepository;
        _priceHistoryRepo = factory.PriceHistoryRepository;

        _repo.Clear();
        _categoryRepo.Clear();
        _priceHistoryRepo.Clear();
    }

    [Fact]
    public async Task ChangePrice_Should_Return_NoContent_And_Save_Product()
    {
        var product = new Product(10);
        _repo.Seed(product);

        var request = new ChangePriceRequest(20);

        var response = await _authenticatedClient.PutAsJsonAsync(
            $"/api/products/{product.Id}/price",
            request
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updated = _repo.GetById(product.Id);
        Assert.NotNull(updated);
        Assert.Equal(20, updated.Price);
    }

    [Fact]
    public async Task ChangePrice_Should_Record_PriceHistory()
    {
        var product = new Product(10);
        _repo.Seed(product);

        var request = new ChangePriceRequest(20);

        var response = await _authenticatedClient.PutAsJsonAsync(
            $"/api/products/{product.Id}/price",
            request
        );

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var history = await _priceHistoryRepo.GetByProductIdAsync(product.Id);
        var entry = Assert.Single(history);
        Assert.Equal(10m, entry.OldPrice);
        Assert.Equal(20m, entry.NewPrice);
    }

    [Fact]
    public async Task ChangePrice_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var nonExistentId = Guid.NewGuid();
        var request = new ChangePriceRequest(20);

        var response = await _authenticatedClient.PutAsJsonAsync(
            $"/api/products/{nonExistentId}/price",
            request
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangePrice_Should_Return_BadRequest_When_Price_Is_Invalid()
    {
        var product = new Product(10);
        _repo.Seed(product);

        var request = new ChangePriceRequest(-5);

        var response = await _authenticatedClient.PutAsJsonAsync(
            $"/api/products/{product.Id}/price",
            request
        );

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return_Created_And_Save_Product()
    {
        var request = new CreateProductRequest(
            Name: "Test Product",
            Sku: "TEST-001",
            Price: 99.99m,
            Stock: 10,
            Description: "A test product");

        var response = await _authenticatedClient.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(product);
        Assert.Equal("Test Product", product.Name);
        Assert.Equal("TEST-001", product.Sku);
        Assert.Equal(99.99m, product.Price);
        Assert.Equal(10, product.Stock);
    }

    [Fact]
    public async Task Create_Should_Return_BadRequest_When_Sku_Is_Duplicate()
    {
        var existingProduct = new Product("Existing Product", 50m, new Sku("DUPE-001"));
        _repo.Seed(existingProduct);

        var request = new CreateProductRequest(
            Name: "New Product",
            Sku: "DUPE-001",
            Price: 99.99m);

        var response = await _authenticatedClient.PostAsJsonAsync("/api/products", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return_Product()
    {
        var product = new Product("Test Product", 50m, new Sku("GET-001"));
        _repo.Seed(product);

        var response = await _client.GetAsync($"/api/products/{product.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Test Product", result.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Product_Does_Not_Exist()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_Should_Return_Products_With_Filters()
    {
        _repo.Seed(new Product("Cheap Product", 10m, new Sku("CHEAP-001")));
        _repo.Seed(new Product("Expensive Product", 100m, new Sku("EXPENS-001")));
        _repo.Seed(new Product("Medium Product", 50m, new Sku("MEDIUM-001")));

        var response = await _client.GetAsync("/api/products?minPrice=20&maxPrice=80");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal("Medium Product", result.Items[0].Name);
    }

    [Fact]
    public async Task Search_Should_Return_BadRequest_When_Page_Is_Invalid()
    {
        var response = await _client.GetAsync("/api/products?page=0&pageSize=20");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_Should_Return_BadRequest_When_PageSize_Is_Invalid()
    {
        var response = await _client.GetAsync("/api/products?page=1&pageSize=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return_NoContent()
    {
        var product = new Product("To Delete", 50m, new Sku("DEL-001"));
        _repo.Seed(product);

        var response = await _adminClient.DeleteAsync($"/api/products/{product.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(_repo.GetById(product.Id));
    }

    [Fact]
    public async Task ApplyDiscount_Should_Return_Product_With_Discount()
    {
        var product = new Product("Discounted Product", 100m, new Sku("DISC-001"));
        _repo.Seed(product);

        var request = new ApplyDiscountRequest(25m, "Summer sale");

        var response = await _authenticatedClient.PostAsJsonAsync($"/api/products/{product.Id}/discount", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Equal(25m, result.DiscountPercentage);
        Assert.Equal(75m, result.EffectivePrice);
    }

    [Fact]
    public async Task RemoveDiscount_Should_Remove_Discount_From_Product()
    {
        var product = new Product("Discounted Product", 100m, new Sku("RDISC-001"));
        product.ApplyDiscount(25m);
        _repo.Seed(product);

        var response = await _authenticatedClient.DeleteAsync($"/api/products/{product.Id}/discount");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Null(result.DiscountPercentage);
        Assert.Equal(100m, result.EffectivePrice);
    }

    [Fact]
    public async Task GetPriceHistory_Should_Return_History()
    {
        var product = new Product("History Product", 100m, new Sku("HIST-001"));
        _repo.Seed(product);

        var priceHistory = new PriceHistory(
            productId: product.Id,
            oldPrice: 100m,
            newPrice: 80m,
            discountPercentage: 20m,
            reason: "Test discount");
        await _priceHistoryRepo.SaveAsync(priceHistory);

        var response = await _client.GetAsync($"/api/products/{product.Id}/price-history");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<List<PriceHistoryResponse>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(100m, result[0].OldPrice);
        Assert.Equal(80m, result[0].NewPrice);
    }

    [Fact]
    public async Task Activate_Should_Activate_Product()
    {
        var product = new Product("Inactive Product", 50m, new Sku("ACT-001"));
        product.Deactivate();
        _repo.Seed(product);

        var response = await _authenticatedClient.PostAsync($"/api/products/{product.Id}/activate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task Deactivate_Should_Deactivate_Product()
    {
        var product = new Product("Active Product", 50m, new Sku("DEACT-001"));
        _repo.Seed(product);

        var response = await _authenticatedClient.PostAsync($"/api/products/{product.Id}/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task Update_Should_Clear_Category_When_ClearCategory_True()
    {
        var category = new Category("Electronics", "Devices");
        await _categoryRepo.SaveAsync(category);

        var product = new Product("Categorized Product", 99m, new Sku("CLR-CAT-001"), categoryId: category.Id);
        _repo.Seed(product);

        var response = await _authenticatedClient.PutAsJsonAsync(
            $"/api/products/{product.Id}",
            new { clearCategory = true }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(result);
        Assert.Null(result.CategoryId);

        var updated = _repo.GetById(product.Id);
        Assert.NotNull(updated);
        Assert.Null(updated.CategoryId);
    }
}
