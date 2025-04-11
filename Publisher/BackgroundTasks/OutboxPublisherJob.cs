using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Publisher.BackgroundTasks;

public class OutboxPublisherJob(IServiceProvider provider, IConnectionMultiplexer redis) : BackgroundService
{
    private readonly IServiceProvider _provider = provider;
    private readonly IConnectionMultiplexer _redis = redis;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var subscriber = _redis.GetSubscriber();
            
            var messages = await context.OutboxMessages
                .Where(x => x.ProcessedAt == null)
                .OrderBy(x => x.CreatedAt)
                .Take(10)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                await subscriber.PublishAsync(RedisChannel.Literal(message.Type), message.Payload);
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}