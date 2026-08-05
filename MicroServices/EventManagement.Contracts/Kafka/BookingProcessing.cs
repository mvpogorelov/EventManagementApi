namespace EventManagement.Contracts.Kafka;

public record BookingProcessing
{
    public Guid BookingId { get; init; }
    public Guid EventId { get; init; }
    public Guid UserId { get; init; }
}
