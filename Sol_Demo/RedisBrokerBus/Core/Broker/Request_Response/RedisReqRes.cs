using System.Text.Json;

namespace RedisBrokerBus.Core.Broker.Request_Response;

public class RequestMessage<T>
{
    public string ResponseChannel { get; set; }
    public T Request { get; set; }
}

public interface IRedisRequest
{
    Task<TResponse> RequestAsync<TRequest, TResponse>(string channel, TRequest request, TimeSpan timeout);
}

public class RedisRequest : IRedisRequest
{
    private readonly ConnectionMultiplexer _redisConnection;

    private readonly ISubscriber _subscriber;

    public RedisRequest(string connectionString, Action<ConfigurationOptions>? configure = null)
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

    async Task<TResponse> IRedisRequest.RequestAsync<TRequest, TResponse>(string channel, TRequest request, TimeSpan timeout)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (_subscriber is null)
            throw new ArgumentNullException(nameof(_subscriber));

        var responseChannel = Guid.NewGuid().ToString();

        var tcs = new TaskCompletionSource<TResponse>();

        await _subscriber.SubscribeAsync(responseChannel, (channel, message) =>
        {
            var response = JsonSerializer.Deserialize<TResponse>(message);
            tcs.SetResult(response);
        });

        var requestMessage = new RequestMessage<TRequest>
        {
            ResponseChannel = responseChannel,
            Request = request
        };

        string json = JsonSerializer.Serialize(requestMessage);
        await _subscriber.PublishAsync(channel, json);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout));

        if (completedTask != tcs.Task)
            throw new TimeoutException();

        return await tcs.Task;
    }
}

public interface IRedisResponse
{
    Task ResponseAsync<TRequest, TResponse>(string channel, Func<TRequest, Task<TResponse>> handler);
}

public class RedisResponse : IRedisResponse
{
    private readonly ConnectionMultiplexer _redisConnection;

    private readonly ISubscriber _subscriber;

    public RedisResponse(string connectionString, Action<ConfigurationOptions>? configure = null)
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

    async Task IRedisResponse.ResponseAsync<TRequest, TResponse>(string channel, Func<TRequest, Task<TResponse>> handler)
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (_subscriber is null)
            throw new ArgumentNullException(nameof(_subscriber));

        await _subscriber.SubscribeAsync(channel, async (channel, message) =>
        {
            var requestMessage = JsonSerializer.Deserialize<RequestMessage<TRequest>>(message);
            var response = await handler(requestMessage.Request);
            var responseJson = JsonSerializer.Serialize(response);
            await _subscriber.PublishAsync(requestMessage.ResponseChannel, responseJson);
        });
    }
}