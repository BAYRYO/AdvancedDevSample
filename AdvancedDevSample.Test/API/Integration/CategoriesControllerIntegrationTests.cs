using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;

namespace AdvancedDevSample.Test.API.Integration;

public class CategoriesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _authenticatedClient;
    private readonly HttpClient _adminClient;
    private readonly InMemoryCategoryRepository _repo;

    public CategoriesControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authenticatedClient = factory.CreateAuthenticatedClient(UserRole.User);
        _adminClient = factory.CreateAuthenticatedClient(UserRole.Admin);
        _repo = factory.CategoryRepository;
        _repo.Clear();
    }

    [Fact]
    public async Task Create_Should_Return_Created_And_Save_Category()
    {
        var request = new CreateCategoryRequest("Electronics", "Devices");

        HttpResponseMessage response = await _authenticatedClient.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CategoryResponse? result = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(result);
        Assert.Equal("Electronics", result.Name);
        Assert.Equal("Devices", result.Description);

        Category? saved = await _repo.GetByIdAsync(result.Id);
        Assert.NotNull(saved);
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized_When_Not_Authenticated()
    {
        var request = new CreateCategoryRequest("Electronics", "Devices");

        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return_Category()
    {
        var category = new Category("Books", "Reading");
        _repo.Seed(category);

        HttpResponseMessage response = await _client.GetAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        CategoryResponse? result = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(result);
        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Books", result.Name);
    }

    [Fact]
    public async Task GetById_Should_Return_NotFound_When_Category_Does_Not_Exist()
    {
        HttpResponseMessage response = await _client.GetAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_WithActiveOnlyTrue_Should_Return_Only_Active_Categories()
    {
        var active = new Category("Active", "Visible");
        var inactive = new Category("Inactive", "Hidden");
        inactive.Deactivate();
        _repo.Seed(active);
        _repo.Seed(inactive);

        HttpResponseMessage response = await _client.GetAsync("/api/categories?activeOnly=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<CategoryResponse>? result = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.NotNull(result);
        CategoryResponse onlyCategory = Assert.Single(result);
        Assert.Equal(active.Id, onlyCategory.Id);
        Assert.True(onlyCategory.IsActive);
    }

    [Fact]
    public async Task Update_Should_Update_Name_Description_And_Status()
    {
        var category = new Category("Old Name", "Old Description");
        _repo.Seed(category);

        var request = new UpdateCategoryRequest(
            Name: "New Name",
            Description: "New Description",
            IsActive: false);

        HttpResponseMessage response = await _authenticatedClient.PutAsJsonAsync($"/api/categories/{category.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        CategoryResponse? result = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.False(result.IsActive);

        Category? saved = await _repo.GetByIdAsync(category.Id);
        Assert.NotNull(saved);
        Assert.Equal("New Name", saved.Name);
        Assert.False(saved.IsActive);
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Category_Does_Not_Exist()
    {
        var request = new UpdateCategoryRequest(Name: "New Name");

        HttpResponseMessage response = await _authenticatedClient.PutAsJsonAsync($"/api/categories/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return_NoContent_And_Remove_Category()
    {
        var category = new Category("To Delete", "Temp");
        _repo.Seed(category);

        HttpResponseMessage response = await _adminClient.DeleteAsync($"/api/categories/{category.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(await _repo.GetByIdAsync(category.Id));
    }

    [Fact]
    public async Task Delete_Should_Return_BadRequest_When_Category_Does_Not_Exist()
    {
        HttpResponseMessage response = await _adminClient.DeleteAsync($"/api/categories/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
