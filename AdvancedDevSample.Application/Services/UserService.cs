using AdvancedDevSample.Application.DTOs.Auth;
using AdvancedDevSample.Application.DTOs.User;
using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Enums;
using AdvancedDevSample.Domain.Exceptions;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponse>> GetAllUsersAsync(int page = 1, int pageSize = 20)
    {
        IEnumerable<User> users = await _userRepository.GetAllAsync(page, pageSize);
        int totalCount = await _userRepository.GetCountAsync();

        IEnumerable<UserResponse> userResponses = users.Select(user => new UserResponse(
            Id: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            Role: user.Role.ToString(),
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt,
            LastLoginAt: user.LastLoginAt));

        return new PagedResult<UserResponse>(
            Items: [.. userResponses],
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize);
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        User? user = await _userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

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

    public async Task<UserResponse> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out UserRole newRole))
        {
            throw new DomainException($"Invalid role: {request.Role}. Valid roles are: User, Admin");
        }

        user.ChangeRole(newRole);
        await _userRepository.SaveAsync(user);

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

    public async Task<UserResponse> DeactivateUserAsync(Guid userId)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        user.Deactivate();
        await _userRepository.SaveAsync(user);

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
