using System.Net;

namespace AdvancedDevSample.Test.API.Integration;

public class MetricsEndpointIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MetricsEndpointIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task MetricsEndpoint_ShouldReturnPrometheusPayload()
    {
        HttpResponseMessage response = await _client.GetAsync("/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("#", payload, StringComparison.Ordinal);
    }
}
