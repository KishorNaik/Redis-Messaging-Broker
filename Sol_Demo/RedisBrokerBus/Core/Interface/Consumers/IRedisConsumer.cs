namespace RedisBrokerBus.Core.Interface.Consumers;

public interface IRedisConsumer<T>
{
    Task HandleAsync(string channel, T message);
}