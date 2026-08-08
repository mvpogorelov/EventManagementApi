namespace EventManagement.Shared.Entities;

public sealed class Outbox
{
    public int Id { get; set; }
    public required string Topic { get; set; }
    public required string MessageKey { get; set; }
    public required string Message { get; set; }
    public required string MessageType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? Error { get; set; }
    public int AttemptCount { get; set; }
}
