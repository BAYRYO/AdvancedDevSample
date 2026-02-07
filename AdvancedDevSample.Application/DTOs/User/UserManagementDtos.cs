using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.User;

public record UpdateUserRoleRequest(
    [Required]
    [RegularExpression("(?i)^(User|Admin)$", ErrorMessage = "Role must be User or Admin.")]
    string Role);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
