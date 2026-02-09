namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking authentication events.
/// </summary>
public class AuditLog
{
    public sealed class ReconstitutionData
    {
        public Guid Id { get; init; }
        public string EventType { get; init; } = string.Empty;
        public Guid? UserId { get; init; }
        public string? UserEmail { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public bool IsSuccess { get; init; }
        public string? Details { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    public Guid Id { get; private set; }
    public string EventType { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? Details { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor for creating new audit log entries
    public AuditLog(
        string eventType,
        Guid? userId,
        string? userEmail,
        string? ipAddress,
        string? userAgent,
        bool isSuccess,
        string? details = null)
    {
        Id = Guid.NewGuid();
        EventType = eventType;
        UserId = userId;
        UserEmail = userEmail;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsSuccess = isSuccess;
        Details = details;
        CreatedAt = DateTime.UtcNow;
    }

    // Constructor for reconstitution from persistence
    public AuditLog(ReconstitutionData data)
    {
        Id = data.Id;
        EventType = data.EventType;
        UserId = data.UserId;
        UserEmail = data.UserEmail;
        IpAddress = data.IpAddress;
        UserAgent = data.UserAgent;
        IsSuccess = data.IsSuccess;
        Details = data.Details;
        CreatedAt = data.CreatedAt;
    }

    // Common event types
    public static class EventTypes
    {
        public const string LoginSuccess = "LOGIN_SUCCESS";
        public const string LoginFailure = "LOGIN_FAILURE";
        public const string Register = "REGISTER";
        public const string TokenRefresh = "TOKEN_REFRESH";
        public const string Logout = "LOGOUT";
    }
}
