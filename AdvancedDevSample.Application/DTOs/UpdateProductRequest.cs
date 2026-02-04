namespace AdvancedDevSample.Application.DTOs;

public record UpdateProductRequest(
    string? Name = null,
    string? Description = null,
    decimal? Price = null,
    int? Stock = null,
    Guid? CategoryId = null,
    bool? IsActive = null);
