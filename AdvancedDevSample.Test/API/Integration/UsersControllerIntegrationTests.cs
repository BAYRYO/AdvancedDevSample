using System.Net;
using System.Net.Http.Json;
using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.DTOs.User;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;

namespace AdvancedDevSample.Test.API.Integration;

public class UsersControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _anonymousClient;

    public UsersControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
        factory.UserRepository.Clear();
    }

    [Fact]
    public async Task GetAllUsers_WithoutToken_Returns401Unauthorized()
    {
        // Act
        HttpResponseMessage response = await _anonymousClient.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_WithNonAdminToken_Returns403Forbidden()
    {
        // Arrange
        HttpClient userClient = _factory.CreateAuthenticatedClient(UserRole.User);

        // Act
        HttpResponseMessage response = await userClient.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_WithAdminToken_ClampsPaginationValues()
    {
        // Arrange
        HttpClient adminClient = _factory.CreateAuthenticatedClient(UserRole.Admin);
        await _factory.UserRepository.SaveAsync(CreateUser("beta@example.com", "Beta", "User"));
        await _factory.UserRepository.SaveAsync(CreateUser("alpha@example.com", "Alpha", "User"));

        // Act
        HttpResponseMessage response = await adminClient.GetAsync("/api/users?page=0&pageSize=500");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PagedResult<UserResponse>? payload = await response.Content.ReadFromJsonAsync<PagedResult<UserResponse>>();
        Assert.NotNull(payload);
        Assert.Equal(1, payload.Page);
        Assert.Equal(100, payload.PageSize);
        Assert.Equal(2, payload.TotalCount);
        Assert.Equal(2, payload.Items.Count);
        Assert.Equal("alpha@example.com", payload.Items[0].Email);
    }

    [Fact]
    public async Task GetUserById_WithMissingUser_Returns404NotFound()
    {
        // Arrange
        HttpClient adminClient = _factory.CreateAuthenticatedClient(UserRole.Admin);

        // Act
        HttpResponseMessage response = await adminClient.GetAsync($"/api/users/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserRole_WithAdminToken_UpdatesRole()
    {
        // Arrange
        HttpClient adminClient = _factory.CreateAuthenticatedClient(UserRole.Admin);
        User user = CreateUser("role-change@example.com", "Role", "Change");
        await _factory.UserRepository.SaveAsync(user);

        // Act
        HttpResponseMessage response = await adminClient.PutAsJsonAsync(
            $"/api/users/{user.Id}/role",
            new UpdateUserRoleRequest(Role: "Admin"));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? payload = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Admin", payload.Role);
    }

    [Fact]
    public async Task DeactivateUser_WithAdminToken_DeactivatesUser()
    {
        // Arrange
        HttpClient adminClient = _factory.CreateAuthenticatedClient(UserRole.Admin);
        User user = CreateUser("deactivate-api@example.com", "Deactivate", "Api");
        await _factory.UserRepository.SaveAsync(user);

        // Act
        HttpResponseMessage response = await adminClient.DeleteAsync($"/api/users/{user.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? payload = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(payload);
        Assert.False(payload.IsActive);
    }

    private static User CreateUser(string email, string firstName, string lastName)
    {
        return new User(new User.ReconstitutionData
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = "hash",
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            LastLoginAt = null
        });
    }
}
