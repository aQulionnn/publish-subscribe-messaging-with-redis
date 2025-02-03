using System.Text.Json;
using Contracts;
using StackExchange.Redis;

namespace Subscriber;

public class Consumer : BackgroundService
{
    private static readonly string ConnectionString = "localhost:6379";
    private static readonly ConnectionMultiplexer Connection = ConnectionMultiplexer.Connect(ConnectionString);
    private const string Channel = "test";
    private readonly ILogger _logger;

    public Consumer(ILogger<Consumer> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = Connection.GetSubscriber();
        subscriber.SubscribeAsync(Channel, (channel, message) =>
        {
            var json = JsonSerializer.Deserialize<Message>(message);
            _logger.LogInformation("Recieved message: {Channel} {@Message}", channel, json);
        });
    }
}