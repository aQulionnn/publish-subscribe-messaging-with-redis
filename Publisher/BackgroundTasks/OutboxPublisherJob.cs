using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Publisher.BackgroundTasks;

public class OutboxPublisherJob(IServiceProvider provider, IConnectionMultiplexer redis)
{
    private readonly IServiceProvider _provider = provider;
    private readonly IConnectionMultiplexer _redis = redis;
    
    public async Task PublishOutboxMessagesAsync()
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var subscriber = _redis.GetSubscriber();
        
        var messages = await context.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.CreatedAt)
            .Take(10)
            .ToListAsync();

        foreach (var message in messages)
        {
            await subscriber.PublishAsync(RedisChannel.Literal(message.Type), message.Payload);
        }
    }
}