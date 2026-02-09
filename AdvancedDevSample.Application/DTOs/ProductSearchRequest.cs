using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs;

public record ProductSearchRequest
{
    public string? Name { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public Guid? CategoryId { get; init; }
    public bool? IsActive { get; init; }

    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}
