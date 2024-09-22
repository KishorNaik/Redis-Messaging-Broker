namespace RedisBrokerBus.Core.Interface.Responders;

public interface IRedisResponder<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}