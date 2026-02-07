using System.ComponentModel.DataAnnotations;

namespace AdvancedDevSample.Application.DTOs.Auth;

public record LoginRequest(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password);
