namespace EventManagement.Contracts.Kafka;

public record BookingRemoved
{
    public Guid BookingId { get; init; }
}
