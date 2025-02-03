using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Publisher;

[Route("api/[controller]")]
[ApiController]
public class ProducerController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;

    public ProducerController(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }
    
    [HttpPost]
    public async Task<IActionResult> PublishMessage()
    {
        var message = new { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow };
        string jsonMessage = JsonSerializer.Serialize(message);

        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync("test", jsonMessage);

        return Ok(new { Message = "Message published" });
    }
}