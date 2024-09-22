using RedisBrokerBus.Core.Broker.Producer_Consumer;
using RedisBrokerBus.Core.Broker.Request_Response;
using RedisBrokerBus.Core.Interface.Responders;

namespace RedisBrokerBus.Extensions.HostedService;

public class ResponderHostedService<TRequest, TResponse> : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _channel;

    public ResponderHostedService(IServiceProvider serviceProvider, IOptions<ConsumerOptions> options)
    {
        _serviceProvider = serviceProvider;
        _channel = options.Value.Channel;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var subscriber = scope.ServiceProvider.GetRequiredService<IRedisResponse>();
            await subscriber.ResponseAsync<TRequest, TResponse>(_channel, async request =>
            {
                using (var innerScope = _serviceProvider.CreateScope())
                {
                    var handler = innerScope.ServiceProvider.GetRequiredService<IResponder<TRequest, TResponse>>();
                    return await handler.HandleAsync(request);
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}