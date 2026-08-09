namespace EventManagement.Contracts.Kafka;

public class BookingRemoved
{
    public Guid BookingId { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
    public int Seats { get; init; }
    public DateTimeOffset RemovedAt { get; init; }
}
