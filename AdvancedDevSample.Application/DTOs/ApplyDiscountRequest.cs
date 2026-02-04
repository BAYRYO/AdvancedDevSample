using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs;

public record ApplyDiscountRequest(
    [Required] decimal Percentage,
    string? Reason = null);
