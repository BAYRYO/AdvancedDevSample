using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs;

public record CreateCategoryRequest(
    [Required] string Name,
    string? Description = null);

public record UpdateCategoryRequest(
    string? Name = null,
    string? Description = null,
    bool? IsActive = null);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
