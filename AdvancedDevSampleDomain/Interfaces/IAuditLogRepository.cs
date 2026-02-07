using AdvancedDevSample.Domain.Entities;

namespace AdvancedDevSample.Domain.Interfaces;

public interface IAuditLogRepository
{
    Task SaveAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 50);
    Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
}
