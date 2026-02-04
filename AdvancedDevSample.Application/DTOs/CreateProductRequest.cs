using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs;

public record CreateProductRequest(
    [Required] string Name,
    [Required] string Sku,
    [Required] decimal Price,
    int Stock = 0,
    string? Description = null,
    Guid? CategoryId = null);
