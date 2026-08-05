namespace BookingsService.Application.Abstractions.Services;

public interface IKafkaProducerService
{
    Task PublishAsync<T>(
        string topic,
        string key,
        T message) where T : class;
}