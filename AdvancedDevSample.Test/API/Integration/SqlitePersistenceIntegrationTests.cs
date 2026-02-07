using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs;
using AdvancedDevSample.Application.DTOs.Auth;

namespace AdvancedDevSample.Test.API.Integration;

public class SqlitePersistenceIntegrationTests : IClassFixture<SqliteWebApplicationFactory>
{
    private readonly SqliteWebApplicationFactory _factory;

    public SqlitePersistenceIntegrationTests(SqliteWebApplicationFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabaseAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Register_DuplicateEmail_DifferentCase_ReturnsConflict_WithRealSqlite()
    {
        var client = _factory.CreateClient();

        var first = new RegisterRequest(
            Email: "CaseSensitive@example.com",
            Password: "Password123!",
            FirstName: "Case",
            LastName: "One");

        var second = new RegisterRequest(
            Email: "casesensitive@example.com",
            Password: "Password123!",
            FirstName: "Case",
            LastName: "Two");

        var firstResponse = await client.PostAsJsonAsync("/api/auth/register", first);
        var secondResponse = await client.PostAsJsonAsync("/api/auth/register", second);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetProduct_Persists_WithRealSqlite()
    {
        var client = _factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            Email: "owner@example.com",
            Password: "Password123!",
            FirstName: "Owner",
            LastName: "User"));

        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseWithRefreshToken>();
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var createResponse = await client.PostAsJsonAsync("/api/products", new CreateProductRequest(
            Name: "SQLite Product",
            Sku: "SQL-001",
            Price: 10m));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("SQLite Product", fetched.Name);
        Assert.Equal("SQL-001", fetched.Sku);
        Assert.Equal(10m, fetched.Price);
    }
}
