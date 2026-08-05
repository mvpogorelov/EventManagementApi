namespace EventManagement.Contracts.Kafka;

public record BookingCancelled
{
    public Guid BookingId { get; init; }
}
