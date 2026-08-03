using BookingsService.Domain.Common;
using BookingsService.Domain.Exceptions;

namespace BookingsService.Domain.Entities;

/// <summary>
/// Бронь
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
    /// Текущий статус брони
    /// </summary>
    public required BookingStatus Status { get; set; }

    /// <summary>
    /// Дата и время создания брони
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Дата и время обработки брони
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    public required Guid UserId { get; init; }
    
    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }
    
    public void Processing()
    {
        Status = BookingStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
        {
            throw new OperationNotAllowedException("Бронь нельзя отменить");
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }
}