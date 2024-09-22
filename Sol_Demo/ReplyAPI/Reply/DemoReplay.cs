using Message.Request_Response;
using RedisBrokerBus.Core.Interface.Responders;

namespace ReplyAPI.Reply;

public class DemoReplay : IResponder<DemoRequest, DemoResponse>
{
    Task<DemoResponse> IResponder<DemoRequest, DemoResponse>.HandleAsync(DemoRequest request)
    {
        var response = new DemoResponse();
        response.Id = Guid.NewGuid();
        response.Name = request.Name;

        Console.WriteLine($"Reply Message: {response.Id} - {response.Name}");

        return Task.FromResult(response);
    }
}