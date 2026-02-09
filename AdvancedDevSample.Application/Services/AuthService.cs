using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Application.Validators;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseWithRefreshToken> RegisterAsync(RegisterRequest request)
    {
        // Validate password strength
        PasswordValidator.Validate(request.Password);

        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new UserAlreadyExistsException(request.Email);
        }

        string passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            email: request.Email,
            passwordHash: passwordHash,
            firstName: request.FirstName,
            lastName: request.LastName);

        await _userRepository.SaveAsync(user);

        (string token, DateTime expiresAt) = _jwtService.GenerateToken(user);
        var refreshToken = new RefreshToken(user.Id);
        await _refreshTokenRepository.SaveAsync(refreshToken);

        return new AuthResponseWithRefreshToken(
            Token: token,
            ExpiresAt: expiresAt,
            RefreshToken: refreshToken.GetPlainTextTokenOrThrow(),
            RefreshTokenExpiresAt: refreshToken.ExpiresAt,
            User: MapToUserResponse(user));
    }

    public async Task<AuthResponseWithRefreshToken> LoginAsync(LoginRequest request)
    {
        User? user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        user.UpdateLastLogin();
        await _userRepository.SaveAsync(user);

        // Revoke any existing refresh tokens for this user
        await _refreshTokenRepository.RevokeAllForUserAsync(user.Id);

        (string token, DateTime expiresAt) = _jwtService.GenerateToken(user);
        var refreshToken = new RefreshToken(user.Id);
        await _refreshTokenRepository.SaveAsync(refreshToken);

        return new AuthResponseWithRefreshToken(
            Token: token,
            ExpiresAt: expiresAt,
            RefreshToken: refreshToken.GetPlainTextTokenOrThrow(),
            RefreshTokenExpiresAt: refreshToken.ExpiresAt,
            User: MapToUserResponse(user));
    }

    public async Task<AuthResponseWithRefreshToken> RefreshTokenAsync(RefreshTokenRequest request)
    {
        RefreshToken? existingRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

        if (existingRefreshToken == null || !existingRefreshToken.IsValid)
        {
            throw new InvalidCredentialsException();
        }

        User? user = await _userRepository.GetByIdAsync(existingRefreshToken.UserId);

        if (user == null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        // Revoke the old refresh token
        existingRefreshToken.Revoke();
        await _refreshTokenRepository.SaveAsync(existingRefreshToken);

        // Generate new tokens
        (string token, DateTime expiresAt) = _jwtService.GenerateToken(user);
        var newRefreshToken = new RefreshToken(user.Id);
        await _refreshTokenRepository.SaveAsync(newRefreshToken);

        return new AuthResponseWithRefreshToken(
            Token: token,
            ExpiresAt: expiresAt,
            RefreshToken: newRefreshToken.GetPlainTextTokenOrThrow(),
            RefreshTokenExpiresAt: newRefreshToken.ExpiresAt,
            User: MapToUserResponse(user));
    }

    public async Task<UserResponse?> GetCurrentUserAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        return MapToUserResponse(user);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt);
    }
}

