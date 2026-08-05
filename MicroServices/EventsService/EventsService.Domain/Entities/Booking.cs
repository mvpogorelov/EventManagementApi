namespace EventsService.Domain.Entities;

/// <summary>
/// Фиксация факта брони для события
/// </summary>
public sealed class Booking
{
    /// <summary>
    /// Уникальный идентификатор брони
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Идентификатор события, к которому относится бронь
    /// </summary>
    public required Guid EventId { get; init; }

    /// <summary>
    /// Событие, к которому относится бронь
    /// </summary>
    public Event? Event { get; init; }

    public required Guid UserId { get; init; }
}
