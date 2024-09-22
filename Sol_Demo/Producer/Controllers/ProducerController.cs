using Message.Consumer_Producer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisBrokerBus.Core.Broker.Producer_Consumer;

namespace Producer.Controllers
{
    [Route("api/producer")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        private readonly IRedisPublisher _redisPublisher = null;

        public ProducerController(IRedisPublisher redisPublisher)
        {
            _redisPublisher = redisPublisher;
        }

        [HttpPost]
        public async Task<IActionResult> PublishMessage([FromBody] DemoMessage message)
        {
            Console.WriteLine($"Producer Message: {message.Message}");
            await _redisPublisher.PublishAsync<DemoMessage>("demo-channel", message);
            return Ok();
        }
    }
}