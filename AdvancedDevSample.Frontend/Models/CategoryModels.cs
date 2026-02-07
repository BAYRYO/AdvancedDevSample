using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Frontend.Models;

public record CreateCategoryRequest([Required] string Name, string? Description = null);

public record UpdateCategoryRequest(string? Name = null, string? Description = null, bool? IsActive = null);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class CreateCategoryFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public CreateCategoryRequest ToRequest() => new(Name, Description);
}

public class UpdateCategoryFormModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public UpdateCategoryRequest ToRequest() => new(Name, Description, IsActive);
}
