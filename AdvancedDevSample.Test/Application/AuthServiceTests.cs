using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.Interfaces;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.Application;

public class AuthServiceTests
{
    private readonly FakeUserRepository _userRepository;
    private readonly FakeRefreshTokenRepository _refreshTokenRepository;
    private readonly FakePasswordHasher _passwordHasher;
    private readonly FakeJwtService _jwtService;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepository = new FakeUserRepository();
        _refreshTokenRepository = new FakeRefreshTokenRepository();
        _passwordHasher = new FakePasswordHasher();
        _jwtService = new FakeJwtService();
        _authService = new AuthService(_userRepository, _refreshTokenRepository, _passwordHasher, _jwtService);
    }

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_CreatesUserAndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "John",
            LastName: "Doe");

        // Act
        var response = await _authService.RegisterAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.Equal("test@example.com", response.User.Email);
        Assert.Equal("John", response.User.FirstName);
        Assert.Equal("Doe", response.User.LastName);
        Assert.Equal("User", response.User.Role);
        Assert.True(response.User.IsActive);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var existingUser = new User(
            email: "existing@example.com",
            passwordHash: "hashed",
            firstName: "Existing",
            lastName: "User");
        await _userRepository.SaveAsync(existingUser);

        var request = new RegisterRequest(
            Email: "existing@example.com",
            Password: "Password123!",
            FirstName: "John",
            LastName: "Doe");

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() =>
            _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "Password123!",
            FirstName: "John",
            LastName: "Doe");

        // Act
        await _authService.RegisterAsync(request);

        // Assert
        Assert.True(_passwordHasher.HashWasCalled);
        Assert.Equal("Password123!", _passwordHasher.LastHashedPassword);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        await _userRepository.SaveAsync(user);
        _passwordHasher.SetVerifyResult(true);

        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "correct_password");

        // Act
        var response = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.Equal("test@example.com", response.User.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        await _userRepository.SaveAsync(user);
        _passwordHasher.SetVerifyResult(false);

        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "wrong_password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        user.Deactivate();
        await _userRepository.SaveAsync(user);
        _passwordHasher.SetVerifyResult(true);

        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "correct_password");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_UpdatesLastLoginTime()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        await _userRepository.SaveAsync(user);
        _passwordHasher.SetVerifyResult(true);

        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "correct_password");

        // Act
        var response = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(response.User.LastLoginAt);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsNewTokensAndRevokesOldToken()
    {
        // Arrange
        var user = new User(
            email: "refresh@example.com",
            passwordHash: "hashed_password",
            firstName: "Refresh",
            lastName: "User");
        await _userRepository.SaveAsync(user);

        var existingRefreshToken = new RefreshToken(user.Id);
        string plainTextRefreshToken = existingRefreshToken.GetPlainTextTokenOrThrow();
        await _refreshTokenRepository.SaveAsync(existingRefreshToken);

        var request = new RefreshTokenRequest(RefreshToken: plainTextRefreshToken);

        // Act
        AuthResponseWithRefreshToken response = await _authService.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Token);
        Assert.NotEmpty(response.RefreshToken);
        Assert.NotEqual(plainTextRefreshToken, response.RefreshToken);
        Assert.True(existingRefreshToken.IsRevoked);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = new RefreshTokenRequest(RefreshToken: "invalid_token");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = new User(
            email: "revoked@example.com",
            passwordHash: "hashed_password",
            firstName: "Revoked",
            lastName: "User");
        await _userRepository.SaveAsync(user);

        var refreshToken = new RefreshToken(user.Id);
        string plainTextRefreshToken = refreshToken.GetPlainTextTokenOrThrow();
        refreshToken.Revoke();
        await _refreshTokenRepository.SaveAsync(refreshToken);

        var request = new RefreshTokenRequest(RefreshToken: plainTextRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInactiveUser_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var user = new User(
            email: "inactive-refresh@example.com",
            passwordHash: "hashed_password",
            firstName: "Inactive",
            lastName: "User");
        user.Deactivate();
        await _userRepository.SaveAsync(user);

        var refreshToken = new RefreshToken(user.Id);
        string plainTextRefreshToken = refreshToken.GetPlainTextTokenOrThrow();
        await _refreshTokenRepository.SaveAsync(refreshToken);

        var request = new RefreshTokenRequest(RefreshToken: plainTextRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _authService.RefreshTokenAsync(request));
    }

    #endregion

    #region GetCurrentUserAsync Tests

    [Fact]
    public async Task GetCurrentUserAsync_WithValidActiveUser_ReturnsUser()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        await _userRepository.SaveAsync(user);

        // Act
        var response = await _authService.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("test@example.com", response.Email);
        Assert.Equal("Test", response.FirstName);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var response = await _authService.GetCurrentUserAsync(Guid.NewGuid());

        // Assert
        Assert.Null(response);
    }

    [Fact]
    public async Task GetCurrentUserAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var user = new User(
            email: "test@example.com",
            passwordHash: "hashed_password",
            firstName: "Test",
            lastName: "User");
        user.Deactivate();
        await _userRepository.SaveAsync(user);

        // Act
        var response = await _authService.GetCurrentUserAsync(user.Id);

        // Assert
        Assert.Null(response);
    }

    #endregion

    #region Test Fakes

    private class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public Task<User?> GetByIdAsync(Guid id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = _users.Values.FirstOrDefault(u => u.Email == normalizedEmail);
            return Task.FromResult(user);
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var exists = _users.Values.Any(u => u.Email == normalizedEmail);
            return Task.FromResult(exists);
        }

        public Task SaveAsync(User user)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            var users = _users.Values
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);
            return Task.FromResult(users);
        }

        public Task<int> GetCountAsync()
        {
            return Task.FromResult(_users.Count);
        }
    }

    private class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly Dictionary<Guid, RefreshToken> _tokens = new();

        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            RefreshToken? refreshToken = _tokens.Values.FirstOrDefault(t => t.Matches(token));
            return Task.FromResult(refreshToken);
        }

        public Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            var tokens = _tokens.Values.Where(t => t.UserId == userId);
            return Task.FromResult(tokens);
        }

        public Task SaveAsync(RefreshToken refreshToken)
        {
            _tokens[refreshToken.Id] = refreshToken;
            return Task.CompletedTask;
        }

        public Task RevokeAllForUserAsync(Guid userId)
        {
            var userTokens = _tokens.Values.Where(t => t.UserId == userId).ToList();
            foreach (var token in userTokens)
            {
                token.Revoke();
            }
            return Task.CompletedTask;
        }
    }

    private class FakePasswordHasher : IPasswordHasher
    {
        private bool _verifyResult = true;
        public bool HashWasCalled { get; private set; }
        public string? LastHashedPassword { get; private set; }

        public string Hash(string password)
        {
            HashWasCalled = true;
            LastHashedPassword = password;
            return $"hashed_{password}";
        }

        public bool Verify(string password, string hash) => _verifyResult;

        public void SetVerifyResult(bool result) => _verifyResult = result;
    }

    private class FakeJwtService : IJwtService
    {
        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            return ($"fake_token_for_{user.Email}", DateTime.UtcNow.AddHours(1));
        }
    }

    #endregion
}

