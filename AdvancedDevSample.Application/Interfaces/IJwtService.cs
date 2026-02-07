using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Application.Interfaces;

public interface IJwtService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
