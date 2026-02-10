using System.Net;
using System.Text.Json;

namespace AdvancedDevSample.Test.API.Integration;

public class HealthChecksIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthChecksIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task LiveHealthCheck_ShouldReturnHealthy()
    {
        HttpResponseMessage response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);

        Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());
        Assert.True(document.RootElement.GetProperty("checks").TryGetProperty("self", out _));
    }

    [Fact]
    public async Task ReadyHealthCheck_ShouldReturnHealthy_WhenDatabaseIsReachable()
    {
        HttpResponseMessage response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);

        Assert.Equal("Healthy", document.RootElement.GetProperty("status").GetString());
        Assert.True(document.RootElement.GetProperty("checks").TryGetProperty("database", out _));
    }
}
