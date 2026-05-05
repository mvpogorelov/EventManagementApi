namespace EventManagmentApi.Application.Enums
{
    /// <summary>
    /// Статус брони
    /// </summary>
    public enum BookingStatusEnum
    {
        /// <summary>
        /// Бронь создана, ожидает обработки
        /// </summary>
        Pending,

        /// <summary>
        /// Бронь подтверждена
        /// </summary>
        Confirmed,

        /// <summary>
        /// Бронь отклонена
        /// </summary>
        Rejected
    }
}
