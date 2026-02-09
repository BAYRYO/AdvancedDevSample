using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs;

public record ProductSearchRequest(
    string? Name = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    [property: Range(1, int.MaxValue)]
    int Page = 1,
    [property: Range(1, 100)]
    int PageSize = 20);
