namespace RedisBrokerBus.Core.Interface.Responders;

public interface IResponder<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request);
}