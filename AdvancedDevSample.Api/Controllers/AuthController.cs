using System.Security.Claims;
using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AdvancedDevSample.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly AuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AuthService authService,
        AuditService auditService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseWithRefreshToken), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        await TryAuditAsync(() => _auditService.LogRegisterAsync(
            response.User.Id,
            response.User.Email,
            GetClientIpAddress(),
            GetUserAgent()));
        return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token with refresh token.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(AuthResponseWithRefreshToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            await TryAuditAsync(() => _auditService.LogLoginSuccessAsync(
                response.User.Id,
                response.User.Email,
                GetClientIpAddress(),
                GetUserAgent()));

            return Ok(response);
        }
        catch (InvalidCredentialsException)
        {
            await TryAuditAsync(() => _auditService.LogLoginFailureAsync(
                request.Email,
                GetClientIpAddress(),
                GetUserAgent(),
                "Invalid credentials"));

            throw;
        }
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseWithRefreshToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        await TryAuditAsync(() => _auditService.LogTokenRefreshAsync(
            response.User.Id,
            response.User.Email,
            GetClientIpAddress(),
            GetUserAgent()));
        return Ok(response);
    }

    /// <summary>
    /// Gets the current authenticated user's information.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetCurrentUserAsync(userId);

        if (user == null)
        {
            return Unauthorized();
        }

        return Ok(user);
    }

    private string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.ToString();
    }

    private async Task TryAuditAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to write audit log event");
        }
    }
}

