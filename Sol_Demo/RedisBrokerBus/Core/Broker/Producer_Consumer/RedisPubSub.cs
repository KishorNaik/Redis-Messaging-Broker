using System.Text.Json;

namespace RedisBrokerBus.Core.Broker.Producer_Consumer;

public interface IRedisPublisher
{
    Task PublishAsync<T>(string channel, T message);
}

public class RedisPublisher : IRedisPublisher
{
    private readonly ConnectionMultiplexer _redisConnection;

    private readonly ISubscriber _subscriber;

    public RedisPublisher(string connectionString, Action<ConfigurationOptions>? configure = null)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));

        if (connectionString is not null && configure is not null)
        {
            _redisConnection = ConnectionMultiplexer.Connect(connectionString, configure);
        }
        else
        {
            _redisConnection = ConnectionMultiplexer.Connect(connectionString);
        }

        _subscriber = _redisConnection.GetSubscriber();
    }

    async Task IRedisPublisher.PublishAsync<T>(string channel, T message)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        if (message is null)
            throw new ArgumentNullException(nameof(message));

        if (_subscriber is null)
            throw new ArgumentNullException(nameof(_subscriber));

        string json = JsonSerializer.Serialize(message);

        await _subscriber.PublishAsync(channel, json);
    }
}

public interface IRedisSubscriber
{
    Task SubscribeAsync<T>(string channel, Action<T> handler);

    Task UnsubscribeAsync(string channel);
}

public class RedisSubscriber : IRedisSubscriber
{
    private readonly ConnectionMultiplexer _redisConnection;

    private readonly ISubscriber _subscriber;

    public RedisSubscriber(string connectionString, Action<ConfigurationOptions>? configure = null)
    {
        if (connectionString is null)
            throw new ArgumentNullException(nameof(connectionString));

        if (connectionString is not null && configure is not null)
        {
            _redisConnection = ConnectionMultiplexer.Connect(connectionString, configure);
        }
        else
        {
            _redisConnection = ConnectionMultiplexer.Connect(connectionString);
        }
        _subscriber = _redisConnection.GetSubscriber();
    }

    async Task IRedisSubscriber.SubscribeAsync<T>(string channel, Action<T> handler)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (_subscriber is null)
            throw new ArgumentNullException(nameof(_subscriber));

        await _subscriber.SubscribeAsync(channel, (channel, message) =>
        {
            T? messageDeserialized = JsonSerializer.Deserialize<T>(message!);
            handler.Invoke(messageDeserialized!);
        });
    }

    async Task IRedisSubscriber.UnsubscribeAsync(string channel)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        await _subscriber.UnsubscribeAsync(channel);
    }
}