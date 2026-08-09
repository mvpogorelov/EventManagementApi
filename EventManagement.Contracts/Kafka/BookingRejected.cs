namespace EventManagement.Contracts.Kafka;

public record BookingRejected
{
    public Guid BookingId { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset RejectedAt { get; init; }
}
