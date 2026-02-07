using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Test.API.Integration;

public class InMemoryAuditLogRepository : IAuditLogRepository
{
    private readonly List<AuditLog> _auditLogs = new();

    public Task SaveAsync(AuditLog auditLog)
    {
        _auditLogs.Add(auditLog);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        var logs = _auditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit);
        return Task.FromResult(logs);
    }

    public Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
    {
        var logs = _auditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit);
        return Task.FromResult(logs);
    }

    public void Clear()
    {
        _auditLogs.Clear();
    }
}
