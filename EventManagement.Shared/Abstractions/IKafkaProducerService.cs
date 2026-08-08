namespace EventManagement.Shared.Abstractions;

public interface IKafkaProducerService
{
    Task PublishAsync<T>(
        string topic,
        string key,
        T message) where T : class;
}
