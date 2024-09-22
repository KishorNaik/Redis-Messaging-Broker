using Message.Consumer_Producer;
using RedisBrokerBus.Core.Interface.Consumers;

namespace Consumer.Consumers;

public class DemoConsumer : IRedisConsumer<DemoMessage>
{
    Task IRedisConsumer<DemoMessage>.HandleAsync(string channel, DemoMessage message)
    {
        Console.WriteLine($"Consumer Received Channel: {channel}");
        Console.WriteLine($"Consumer Received Message: {message.Message}");

        return Task.CompletedTask;
    }
}