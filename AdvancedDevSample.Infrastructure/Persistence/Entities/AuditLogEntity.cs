namespace AdvancedDevSample.Infrastructure.Persistence.Entities;

public class AuditLogEntity
{
    public Guid Id { get; set; }
    public required string EventType { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsSuccess { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}
