//https://medium.com/innoviletech/redis-pub-sub-with-net-core-758c1d3c7a98
//https://github.com/Vahidalizadeh7070/RedisMessageBrokerMedium/blob/master/RedisBroker/RedisMessageBroker.cs

using RedisBrokerBus.Extensions.Configurations;

namespace RedisBrokerBus.Extensions;

public static class RedisMessageBrokerExtension
{
    public static IServiceCollection AddRedisMessageBroker(this IServiceCollection services, string redisConnectionString, Action<RedisConfiguration> configure)
    {
        if (redisConnectionString is null)
            throw new ArgumentNullException(nameof(redisConnectionString));

        if (configure is null)
            throw new ArgumentNullException(nameof(configure));

        configure.Invoke(new RedisConfiguration(services, redisConnectionString));

        return services;
    }
}