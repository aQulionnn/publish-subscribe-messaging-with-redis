using System.Text.Json;
using Contracts;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using Subscriber.Hubs;

namespace Subscriber;

public class Consumer : BackgroundService
{
    private static readonly string ConnectionString = "localhost:6379";
    private static readonly ConnectionMultiplexer Connection = ConnectionMultiplexer.Connect(ConnectionString);
    private const string Channel = "activity";
    private readonly ILogger _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public Consumer(ILogger<Consumer> logger, IHubContext<NotificationHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = Connection.GetSubscriber();
        await subscriber.SubscribeAsync(Channel, async (channel, message) =>
        {
            var json = JsonSerializer.Deserialize<Message>(message);
            await _hubContext.Clients.All.SendAsync("RecieveNotification", json);
        });
    }
}