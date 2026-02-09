using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;
using AdvancedDevSample.Infrastructure.Persistence;
using AdvancedDevSample.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdvancedDevSample.Infrastructure.Repositories;

public class EfAuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public EfAuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(AuditLog auditLog)
    {
        AuditLogEntity entity = ToEntity(auditLog);
        _context.AuditLogs.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, int limit = 50)
    {
        List<AuditLogEntity> entities = await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(ToDomain);
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
    {
        List<AuditLogEntity> entities = await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return entities.Select(ToDomain);
    }

    private static AuditLog ToDomain(AuditLogEntity entity)
    {
        return new AuditLog(new AuditLog.ReconstitutionData
        {
            Id = entity.Id,
            EventType = entity.EventType,
            UserId = entity.UserId,
            UserEmail = entity.UserEmail,
            IpAddress = entity.IpAddress,
            UserAgent = entity.UserAgent,
            IsSuccess = entity.IsSuccess,
            Details = entity.Details,
            CreatedAt = entity.CreatedAt
        });
    }

    private static AuditLogEntity ToEntity(AuditLog domain)
    {
        return new AuditLogEntity
        {
            Id = domain.Id,
            EventType = domain.EventType,
            UserId = domain.UserId,
            UserEmail = domain.UserEmail,
            IpAddress = domain.IpAddress,
            UserAgent = domain.UserAgent,
            IsSuccess = domain.IsSuccess,
            Details = domain.Details,
            CreatedAt = domain.CreatedAt
        };
    }
}
