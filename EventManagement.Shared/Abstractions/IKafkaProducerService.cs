namespace EventManagement.Shared.Abstractions;

public interface IKafkaProducerService
{
    Task PublishAsync<T>(string topic, string key, T message) where T : class;
    
    Task PublishAsync(string topic, string key, string message, string messageType);

    string GetMessageString<T>(T message) where T : class;

    string GetMessageType<T>(T message) where T : class;
}
