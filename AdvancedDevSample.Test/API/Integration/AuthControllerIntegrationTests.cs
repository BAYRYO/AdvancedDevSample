using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;

namespace AdvancedDevSample.Test.API.Integration;

public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        factory.UserRepository.Clear();
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_Returns201AndToken()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "newuser@example.com",
            Password: "Password123!",
            FirstName: "New",
            LastName: "User");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AuthResponseWithRefreshToken? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseWithRefreshToken>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal("newuser@example.com", authResponse.User.Email);
        Assert.Equal("New", authResponse.User.FirstName);
        Assert.Equal("User", authResponse.User.LastName);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns409Conflict()
    {
        // Arrange
        var existingUser = new User(
            email: "existing@example.com",
            passwordHash: "hashed",
            firstName: "Existing",
            lastName: "User");
        await _factory.UserRepository.SaveAsync(existingUser);

        var request = new RegisterRequest(
            Email: "existing@example.com",
            Password: "Password123!",
            FirstName: "Another",
            LastName: "User");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")]
    [InlineData("invalid", "Password123!", "John", "Doe")]
    [InlineData("test@example.com", "", "John", "Doe")]
    [InlineData("test@example.com", "short", "John", "Doe")]
    [InlineData("test@example.com", "Password123!", "", "Doe")]
    [InlineData("test@example.com", "Password123!", "John", "")]
    public async Task Register_WithInvalidData_Returns400BadRequest(
        string email, string password, string firstName, string lastName)
    {
        // Arrange
        var request = new
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        // First register a user
        var registerRequest = new RegisterRequest(
            Email: "logintest@example.com",
            Password: "Password123!",
            FirstName: "Login",
            LastName: "Test");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Now login
        var loginRequest = new LoginRequest(
            Email: "logintest@example.com",
            Password: "Password123!");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResponseWithRefreshToken? authResponse = await response.Content.ReadFromJsonAsync<AuthResponseWithRefreshToken>();
        Assert.NotNull(authResponse);
        Assert.NotEmpty(authResponse.Token);
        Assert.NotEmpty(authResponse.RefreshToken);
        Assert.Equal("logintest@example.com", authResponse.User.Email);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_Returns401Unauthorized()
    {
        // Arrange
        var request = new LoginRequest(
            Email: "nonexistent@example.com",
            Password: "password");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401Unauthorized()
    {
        // First register a user
        var registerRequest = new RegisterRequest(
            Email: "wrongpass@example.com",
            Password: "CorrectPassword123!",
            FirstName: "Test",
            LastName: "User");
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Now try wrong password
        var loginRequest = new LoginRequest(
            Email: "wrongpass@example.com",
            Password: "WrongPassword123!");

        // Act
        HttpResponseMessage response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidToken_Returns200AndUser()
    {
        // Arrange - Create authenticated client
        var userId = Guid.NewGuid();
        var user = new User(new User.ReconstitutionData
        {
            Id = userId,
            Email = "current@example.com",
            PasswordHash = "hash",
            FirstName = "Current",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = null
        });
        await _factory.UserRepository.SaveAsync(user);

        HttpClient authenticatedClient = _factory.CreateClient();
        string token = _factory.GenerateTestToken(userId, "current@example.com", UserRole.User);
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = await authenticatedClient.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? userResponse = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(userResponse);
        Assert.Equal("current@example.com", userResponse.Email);
        Assert.Equal("Current", userResponse.FirstName);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutToken_Returns401Unauthorized()
    {
        // Act
        HttpResponseMessage response = await _client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentUser_WithInactiveUser_Returns401Unauthorized()
    {
        // Arrange - Create inactive user
        var userId = Guid.NewGuid();
        var user = new User(new User.ReconstitutionData
        {
            Id = userId,
            Email = "inactive@example.com",
            PasswordHash = "hash",
            FirstName = "Inactive",
            LastName = "User",
            Role = UserRole.User,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = null
        });
        await _factory.UserRepository.SaveAsync(user);

        HttpClient authenticatedClient = _factory.CreateClient();
        string token = _factory.GenerateTestToken(userId, "inactive@example.com", UserRole.User);
        authenticatedClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = await authenticatedClient.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion
}
