using System.Net;
using System.Net.Http.Json;
using System.Text;
using AdvancedDevSample.Frontend.Models;
using AdvancedDevSample.Frontend.Services;

namespace AdvancedDevSample.Test.Frontend.Services;

public class ApiClientTests
{
    [Fact]
    public async Task SearchProductsAsync_Should_Send_QueryString_With_Only_NonEmpty_Values()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var payload = new PagedResponse<ProductResponse>(
                Items: [],
                TotalCount: 0,
                Page: 2,
                PageSize: 5,
                TotalPages: 0);
            return Task.FromResult(CreateJsonResponse(HttpStatusCode.OK, payload));
        });

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://example.test") };
        var apiClient = new ApiClient(httpClient);

        await apiClient.SearchProductsAsync(new ProductSearchRequest(
            Name: "Phone Plus",
            MinPrice: 10.5m,
            MaxPrice: null,
            CategoryId: null,
            IsActive: true,
            Page: 2,
            PageSize: 5));

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("/api/products?name=Phone%20Plus&minPrice=10.5&isActive=True&page=2&pageSize=5", handler.LastRequest!.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetCurrentUserAsync_With_Empty_Success_Body_Should_Throw_ApiException()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            }));
        var apiClient = new ApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test") });

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiClient.GetCurrentUserAsync());

        Assert.Equal("The API returned an empty response.", exception.Message);
        Assert.Equal((int)HttpStatusCode.OK, exception.StatusCode);
    }

    [Fact]
    public async Task DeleteProductAsync_With_Empty_Error_Body_Should_Throw_Default_Message()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("   ")
            }));
        var apiClient = new ApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test") });

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiClient.DeleteProductAsync(Guid.NewGuid()));

        Assert.Equal("Request failed with status 400.", exception.Message);
        Assert.Equal((int)HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUserAsync_With_Invalid_Json_Error_Body_Should_Use_Raw_Body_Message()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent("upstream down")
            }));
        var apiClient = new ApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test") });

        ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiClient.GetCurrentUserAsync());

        Assert.Equal("upstream down", exception.Message);
        Assert.Equal((int)HttpStatusCode.BadGateway, exception.StatusCode);
    }

    private static HttpResponseMessage CreateJsonResponse<T>(HttpStatusCode statusCode, T payload)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = JsonContent.Create(payload)
        };
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return _handler(request);
        }
    }
}
