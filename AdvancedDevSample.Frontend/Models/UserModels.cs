using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Frontend.Models;

public record UpdateUserRoleRequest(string Role);

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

public class UpdateUserRoleFormModel
{
    [Required]
    public string Role { get; set; } = "User";

    public UpdateUserRoleRequest ToRequest() => new(Role);
}
