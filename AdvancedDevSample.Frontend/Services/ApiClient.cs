using System.Net.Http.Json;
using System.Text.Json;
using AdvancedDevSample.Frontend.Models;

namespace AdvancedDevSample.Frontend.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResponse<ProductResponse>> SearchProductsAsync(ProductSearchRequest request)
    {
        string query = BuildQuery(new Dictionary<string, string?>
        {
            ["name"] = request.Name,
            ["minPrice"] = request.MinPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["maxPrice"] = request.MaxPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["categoryId"] = request.CategoryId?.ToString(),
            ["isActive"] = request.IsActive?.ToString(),
            ["page"] = request.Page.ToString(),
            ["pageSize"] = request.PageSize.ToString()
        });

        return await GetAsync<PagedResponse<ProductResponse>>($"/api/products{query}");
    }

    public Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        => PostAsync<CreateProductRequest, ProductResponse>("/api/products", request);

    public Task<ProductResponse> ActivateProductAsync(Guid productId)
        => PostAsync<object, ProductResponse>($"/api/products/{productId}/activate", new { });

    public Task<ProductResponse> DeactivateProductAsync(Guid productId)
        => PostAsync<object, ProductResponse>($"/api/products/{productId}/deactivate", new { });

    public Task<ProductResponse> ApplyDiscountAsync(Guid productId, ApplyDiscountRequest request)
        => PostAsync<ApplyDiscountRequest, ProductResponse>($"/api/products/{productId}/discount", request);

    public Task<ProductResponse> RemoveDiscountAsync(Guid productId)
        => DeleteAsync<ProductResponse>($"/api/products/{productId}/discount");

    public Task DeleteProductAsync(Guid productId) => DeleteAsync($"/api/products/{productId}");

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(bool? activeOnly = null)
    {
        string query = activeOnly.HasValue ? $"?activeOnly={activeOnly.Value.ToString().ToLowerInvariant()}" : string.Empty;
        return await GetAsync<IReadOnlyList<CategoryResponse>>($"/api/categories{query}");
    }

    public Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        => PostAsync<CreateCategoryRequest, CategoryResponse>("/api/categories", request);

    public Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request)
        => PutAsync<UpdateCategoryRequest, CategoryResponse>($"/api/categories/{categoryId}", request);

    public Task DeleteCategoryAsync(Guid categoryId) => DeleteAsync($"/api/categories/{categoryId}");

    public Task<PagedResult<UserResponse>> GetUsersAsync(int page, int pageSize)
        => GetAsync<PagedResult<UserResponse>>($"/api/users?page={page}&pageSize={pageSize}");

    public Task<UserResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
        => PutAsync<UpdateUserRoleRequest, UserResponse>($"/api/users/{userId}/role", request);

    public Task<UserResponse> DeactivateUserAsync(Guid userId)
        => DeleteAsync<UserResponse>($"/api/users/{userId}");

    public Task<UserResponse> GetCurrentUserAsync() => GetAsync<UserResponse>("/api/auth/me");

    private async Task<TResponse> GetAsync<TResponse>(string uri)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(uri, request);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uri, request);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> DeleteAsync<TResponse>(string uri)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task DeleteAsync(string uri)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response);
        }
    }

    private static async Task<T> ReadRequiredAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response);
        }

        T? payload = await response.Content.ReadFromJsonAsync<T>();
        return payload ?? throw new ApiException("The API returned an empty response.", (int)response.StatusCode);
    }

    private static async Task<ApiException> CreateApiExceptionAsync(HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return new ApiException($"Request failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            JsonElement root = json.RootElement;
            string? title = root.TryGetProperty("title", out JsonElement titleProp) ? titleProp.GetString() : null;
            string? detail = root.TryGetProperty("detail", out JsonElement detailProp) ? detailProp.GetString() : null;
            string? error = root.TryGetProperty("error", out JsonElement errorProp) ? errorProp.GetString() : null;

            string message = detail ?? title ?? error ?? body;
            return new ApiException(message, (int)response.StatusCode);
        }
        catch
        {
            return new ApiException(body, (int)response.StatusCode);
        }
    }

    private static string BuildQuery(Dictionary<string, string?> values)
    {
        var items = values
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}")
            .ToList();

        return items.Count == 0 ? string.Empty : $"?{string.Join("&", items)}";
    }
}
