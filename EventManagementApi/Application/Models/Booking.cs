using EventManagmentApi.Application.Enums;

namespace EventManagmentApi.Application.Models
{
    /// <summary>
    /// Бронь
    /// </summary>
    public record Booking
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
        public Event Event { get; init; }

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
    }
}
