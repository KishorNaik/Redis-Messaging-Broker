using RedisBrokerBus.Core.Broker.Producer_Consumer;
using RedisBrokerBus.Core.Broker.Request_Response;
using RedisBrokerBus.Core.Interface.Consumers;
using RedisBrokerBus.Core.Interface.Responders;
using RedisBrokerBus.Extensions.HostedService;

namespace RedisBrokerBus.Extensions.Configurations;

public class RedisConfiguration
{
    private readonly IServiceCollection _services;
    private readonly string _redisConnectionString;

    public RedisConfiguration(IServiceCollection services, string redisConnectionString)
    {
        _services = services;
        _redisConnectionString = redisConnectionString;
    }

    public void AddPublisher(Action<ConfigurationOptions>? configure = null)
    {
        if (_redisConnectionString is null)
            throw new ArgumentNullException(nameof(_redisConnectionString));

        _services.AddScoped<IRedisPublisher>(x => new RedisPublisher(_redisConnectionString, configure));
    }

    public void AddSubscriber(Action<ConfigurationOptions>? configure = null)
    {
        if (_redisConnectionString is null)
            throw new ArgumentNullException(nameof(_redisConnectionString));

        _services.AddScoped<IRedisSubscriber>(x => new RedisSubscriber(_redisConnectionString, configure));
    }

    public void AddConsumer<TConsumer, TMessage>(string channel)
    where TConsumer : class, IRedisConsumer<TMessage>
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        _services.AddScoped<IRedisConsumer<TMessage>, TConsumer>();
        _services.AddSingleton<IHostedService, ConsumerHostedService<TMessage>>();
        _services.Configure<ConsumerOptions>(options => options.Channel = channel);
    }

    public void AddRequest(Action<ConfigurationOptions>? configure = null)
    {
        if (_redisConnectionString is null)
            throw new ArgumentNullException(nameof(_redisConnectionString));

        _services.AddScoped<IRedisRequest>(x => new RedisRequest(_redisConnectionString, configure));
    }

    public void AddResponse(Action<ConfigurationOptions>? configure = null)
    {
        if (_redisConnectionString is null)
            throw new ArgumentNullException(nameof(_redisConnectionString));

        _services.AddScoped<IRedisResponse>(x => new RedisResponse(_redisConnectionString, configure));
    }

    public void AddReply<TRequest, TResponse, THandler>(string channel)
    where THandler : class, IResponder<TRequest, TResponse>
    {
        if (channel is null)
            throw new ArgumentNullException(nameof(channel));

        _services.AddScoped<IResponder<TRequest, TResponse>, THandler>();
        _services.AddSingleton<IHostedService, ResponderHostedService<TRequest, TResponse>>();
        _services.Configure<ConsumerOptions>(options => options.Channel = channel);
    }
}