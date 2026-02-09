using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.DTOs.User;
using AdvancedDevSample.Application.Services;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.Application;

public class UserServiceTests
{
    private readonly FakeUserRepository _userRepository;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepository = new FakeUserRepository();
        _userService = new UserService(_userRepository);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsPagedUsers()
    {
        // Arrange
        var firstUser = new User("zeta@example.com", "hash", "Zeta", "User");
        var secondUser = new User("alpha@example.com", "hash", "Alpha", "User");
        await _userRepository.SaveAsync(firstUser);
        await _userRepository.SaveAsync(secondUser);

        // Act
        PagedResult<UserResponse> result = await _userService.GetAllUsersAsync(page: 1, pageSize: 1);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.False(result.HasPreviousPage);
        Assert.Equal("alpha@example.com", result.Items[0].Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var user = new User("test@example.com", "hash", "Test", "User");
        await _userRepository.SaveAsync(user);

        // Act
        UserResponse? result = await _userService.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithUnknownUser_ReturnsNull()
    {
        // Act
        UserResponse? result = await _userService.GetUserByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WithValidRole_UpdatesAndReturnsUser()
    {
        // Arrange
        var user = new User("role@example.com", "hash", "Role", "User");
        await _userRepository.SaveAsync(user);

        var request = new UpdateUserRoleRequest(Role: "Admin");

        // Act
        UserResponse result = await _userService.UpdateUserRoleAsync(user.Id, request);

        // Assert
        Assert.Equal("Admin", result.Role);

        User? persistedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(persistedUser);
        Assert.Equal(UserRole.Admin, persistedUser.Role);
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WithUnknownUser_ThrowsUserNotFoundException()
    {
        // Arrange
        var request = new UpdateUserRoleRequest(Role: "Admin");

        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _userService.UpdateUserRoleAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateUserRoleAsync_WithInvalidRole_ThrowsDomainException()
    {
        // Arrange
        var user = new User("invalidrole@example.com", "hash", "Invalid", "Role");
        await _userRepository.SaveAsync(user);

        var request = new UpdateUserRoleRequest(Role: "SuperAdmin");

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            _userService.UpdateUserRoleAsync(user.Id, request));
    }

    [Fact]
    public async Task DeactivateUserAsync_WithExistingUser_DeactivatesUser()
    {
        // Arrange
        var user = new User("deactivate@example.com", "hash", "Deactivate", "Me");
        await _userRepository.SaveAsync(user);

        // Act
        UserResponse result = await _userService.DeactivateUserAsync(user.Id);

        // Assert
        Assert.False(result.IsActive);

        User? persistedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(persistedUser);
        Assert.False(persistedUser.IsActive);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithUnknownUser_ThrowsUserNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UserNotFoundException>(() =>
            _userService.DeactivateUserAsync(Guid.NewGuid()));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = [];

        public Task<User?> GetByIdAsync(Guid id)
        {
            _users.TryGetValue(id, out User? user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();
            User? user = _users.Values.FirstOrDefault(u => u.Email == normalizedEmail);
            return Task.FromResult(user);
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            string normalizedEmail = email.Trim().ToLowerInvariant();
            bool exists = _users.Values.Any(u => u.Email == normalizedEmail);
            return Task.FromResult(exists);
        }

        public Task SaveAsync(User user)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public Task<IEnumerable<User>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            IEnumerable<User> users = _users.Values
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return Task.FromResult(users.AsEnumerable());
        }

        public Task<int> GetCountAsync() => Task.FromResult(_users.Count);
    }
}
