namespace EventManagement.Contracts.Kafka;

public record BookingRejected
{
    public Guid BookingId { get; init; }
    public string? Reason { get; init; }
}
