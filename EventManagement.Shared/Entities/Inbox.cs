namespace EventManagement.Shared.Entities;

public sealed class Inbox
{
    public int Id { get; set; }
    public required string Topic { get; set; }
    public required string MessageKey { get; set; }
    public required string Message { get; set; }
    public string? MessageType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int AttemptCount { get; set; }
    public Guid? EventId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? BookingId { get; set; }
}
