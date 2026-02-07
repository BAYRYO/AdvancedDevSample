using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Auth;

public record RegisterRequest(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email,

    [Required]
    [MinLength(8)]
    string Password,

    [Required]
    [MaxLength(100)]
    string FirstName,

    [Required]
    [MaxLength(100)]
    string LastName);
