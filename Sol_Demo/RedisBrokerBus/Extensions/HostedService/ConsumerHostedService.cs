using RedisBrokerBus.Core.Broker.Producer_Consumer;
using RedisBrokerBus.Core.Interface.Consumers;

namespace RedisBrokerBus.Extensions.HostedService;

public class ConsumerOptions
{
    public string Channel { get; set; }
}

public class ConsumerHostedService<TMessage> : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _channel;

    public ConsumerHostedService(IServiceProvider serviceProvider, IOptions<ConsumerOptions> options)
    {
        _serviceProvider = serviceProvider;
        _channel = options.Value.Channel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var subscriber = scope.ServiceProvider.GetRequiredService<IRedisSubscriber>();
            await subscriber.SubscribeAsync<TMessage>(_channel, async message =>
            {
                using (var innerScope = _serviceProvider.CreateScope())
                {
                    var consumer = innerScope.ServiceProvider.GetRequiredService<IRedisConsumer<TMessage>>();
                    await consumer.HandleAsync(_channel, message);
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}