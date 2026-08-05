namespace EventManagement.Contracts.Kafka;

public record EventDisabled
{
    public Guid EventId { get; init; }
    public Guid BookingId { get; init; }
    public string Reason { get; init; }
}
