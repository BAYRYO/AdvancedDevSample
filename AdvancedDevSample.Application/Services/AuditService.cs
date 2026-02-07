using AdvancedDevSample.Domain.Entities;
using AdvancedDevSample.Domain.Interfaces;

namespace AdvancedDevSample.Application.Services;

/// <summary>
/// Service for logging authentication events.
/// </summary>
public class AuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogLoginSuccessAsync(
        Guid userId,
        string userEmail,
        string? ipAddress,
        string? userAgent)
    {
        var auditLog = new AuditLog(
            eventType: AuditLog.EventTypes.LoginSuccess,
            userId: userId,
            userEmail: userEmail,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: true);

        await _auditLogRepository.SaveAsync(auditLog);
    }

    public async Task LogLoginFailureAsync(
        string userEmail,
        string? ipAddress,
        string? userAgent,
        string? reason = null)
    {
        var auditLog = new AuditLog(
            eventType: AuditLog.EventTypes.LoginFailure,
            userId: null,
            userEmail: userEmail,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: false,
            details: reason);

        await _auditLogRepository.SaveAsync(auditLog);
    }

    public async Task LogRegisterAsync(
        Guid userId,
        string userEmail,
        string? ipAddress,
        string? userAgent)
    {
        var auditLog = new AuditLog(
            eventType: AuditLog.EventTypes.Register,
            userId: userId,
            userEmail: userEmail,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: true);

        await _auditLogRepository.SaveAsync(auditLog);
    }

    public async Task LogTokenRefreshAsync(
        Guid userId,
        string? userEmail,
        string? ipAddress,
        string? userAgent)
    {
        var auditLog = new AuditLog(
            eventType: AuditLog.EventTypes.TokenRefresh,
            userId: userId,
            userEmail: userEmail,
            ipAddress: ipAddress,
            userAgent: userAgent,
            isSuccess: true);

        await _auditLogRepository.SaveAsync(auditLog);
    }
}
