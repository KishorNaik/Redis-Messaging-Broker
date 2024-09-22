using Message.Request_Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisBrokerBus.Core.Broker.Request_Response;

namespace RequestAPI.Controllers
{
    [Route("api/request")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRedisRequest _redisRequest;

        public RequestController(IRedisRequest redisRequest)
        {
            _redisRequest = redisRequest;
        }

        [HttpPost]
        public async Task<IActionResult> PublishMessage([FromBody] DemoRequest message)
        {
            Console.WriteLine($"Request Message: {message.Name}");
            var response = await _redisRequest.RequestAsync<DemoRequest, DemoResponse>("request-channel", message, TimeSpan.FromSeconds(5));
            Console.WriteLine($"Response Message: {response.Id} - {response.Name}");
            return Ok(response);
        }
    }
}