namespace AdvancedDevSample.Application.DTOs;

public record ProductSearchRequest(
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);
