namespace EventManagement.Contracts.Kafka;

public class BookingPending
{
    public Guid BookingId { get; init; }
    public int Seats { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public DateTimeOffset PendingAt { get; init; }
}
