namespace EventManagement.Contracts.Kafka;

public record EventAllowed
{
    public Guid EventId { get; init; }
    public Guid BookingId { get; init; }
}
