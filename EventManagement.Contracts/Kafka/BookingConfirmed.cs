namespace EventManagement.Contracts.Kafka;

public record BookingConfirmed
{
    public Guid BookingId { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset ConfirmedAt { get; init; }
}
