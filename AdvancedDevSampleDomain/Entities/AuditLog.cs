namespace AdvancedDevSample.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking authentication events.
/// </summary>
public class AuditLog
{
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
    public AuditLog(
        Guid id,
        string eventType,
        Guid? userId,
        string? userEmail,
        string? ipAddress,
        string? userAgent,
        bool isSuccess,
        string? details,
        DateTime createdAt)
    {
        Id = id;
        EventType = eventType;
        UserId = userId;
        UserEmail = userEmail;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        IsSuccess = isSuccess;
        Details = details;
        CreatedAt = createdAt;
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
