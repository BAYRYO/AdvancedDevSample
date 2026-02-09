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
        var query = BuildQuery(new Dictionary<string, string?>
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

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        return await PostAsync<CreateProductRequest, ProductResponse>("/api/products", request);
    }

    public async Task<ProductResponse> ActivateProductAsync(Guid productId)
    {
        return await PostAsync<object, ProductResponse>($"/api/products/{productId}/activate", new { });
    }

    public async Task<ProductResponse> DeactivateProductAsync(Guid productId)
    {
        return await PostAsync<object, ProductResponse>($"/api/products/{productId}/deactivate", new { });
    }

    public async Task<ProductResponse> ApplyDiscountAsync(Guid productId, ApplyDiscountRequest request)
    {
        return await PostAsync<ApplyDiscountRequest, ProductResponse>($"/api/products/{productId}/discount", request);
    }

    public async Task<ProductResponse> RemoveDiscountAsync(Guid productId)
    {
        return await DeleteAsync<ProductResponse>($"/api/products/{productId}/discount");
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        await DeleteAsync($"/api/products/{productId}");
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(bool? activeOnly = null)
    {
        var query = activeOnly.HasValue ? $"?activeOnly={activeOnly.Value.ToString().ToLowerInvariant()}" : string.Empty;
        return await GetAsync<IReadOnlyList<CategoryResponse>>($"/api/categories{query}");
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        return await PostAsync<CreateCategoryRequest, CategoryResponse>("/api/categories", request);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request)
    {
        return await PutAsync<UpdateCategoryRequest, CategoryResponse>($"/api/categories/{categoryId}", request);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        await DeleteAsync($"/api/categories/{categoryId}");
    }

    public async Task<PagedResult<UserResponse>> GetUsersAsync(int page, int pageSize)
    {
        return await GetAsync<PagedResult<UserResponse>>($"/api/users?page={page}&pageSize={pageSize}");
    }

    public async Task<UserResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
    {
        return await PutAsync<UpdateUserRoleRequest, UserResponse>($"/api/users/{userId}/role", request);
    }

    public async Task<UserResponse> DeactivateUserAsync(Guid userId)
    {
        return await DeleteAsync<UserResponse>($"/api/users/{userId}");
    }

    public async Task<UserResponse> GetCurrentUserAsync()
    {
        return await GetAsync<UserResponse>("/api/auth/me");
    }

    private async Task<TResponse> GetAsync<TResponse>(string uri)
    {
        var response = await _httpClient.GetAsync(uri);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(uri, request);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> PutAsync<TRequest, TResponse>(string uri, TRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync(uri, request);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task<TResponse> DeleteAsync<TResponse>(string uri)
    {
        var response = await _httpClient.DeleteAsync(uri);
        return await ReadRequiredAsync<TResponse>(response);
    }

    private async Task DeleteAsync(string uri)
    {
        var response = await _httpClient.DeleteAsync(uri);
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

        var payload = await response.Content.ReadFromJsonAsync<T>();
        if (payload is null)
        {
            throw new ApiException("The API returned an empty response.", (int)response.StatusCode);
        }

        return payload;
    }

    private static async Task<ApiException> CreateApiExceptionAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            return new ApiException($"Request failed with status {(int)response.StatusCode}.", (int)response.StatusCode);
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var title = root.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null;
            var detail = root.TryGetProperty("detail", out var detailProp) ? detailProp.GetString() : null;
            var error = root.TryGetProperty("error", out var errorProp) ? errorProp.GetString() : null;

            var message = detail ?? title ?? error ?? body;
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
