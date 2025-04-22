using System.Text.Json;
using Contracts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Subscriber.Entities;
using Subscriber.Hubs;

namespace Subscriber;

public class Consumer : BackgroundService
{
    private static readonly string ConnectionString = "localhost:6379";
    private static readonly ConnectionMultiplexer Connection = ConnectionMultiplexer.Connect(ConnectionString);
    private const string Channel = "activity";
    private readonly ILogger _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly AppDbContext _context;

    public Consumer(ILogger<Consumer> logger, IHubContext<NotificationHub> hubContext, AppDbContext context)
    {
        _logger = logger;
        _hubContext = hubContext;
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = Connection.GetSubscriber();
        await subscriber.SubscribeAsync(Channel, async (channel, message) =>
        {
            var json = JsonSerializer.Deserialize<Message>(message);
            var messageId = json.Id;

            if (!await IsMessageProcessed(messageId))
            {
                await SaveMessageToInbox(messageId, message);
                await ProcessMessage(json);
                await MarkAsProcessed(messageId);
            }
        });
    }

    private async Task<bool> IsMessageProcessed(Guid messageId)
    {
        return await _context.InboxMessages.AnyAsync(x => x.Id == messageId && x.Processed);
    }

    private async Task SaveMessageToInbox(Guid messageId, string payload)
    {
        await _context.InboxMessages.AddAsync(new InboxMessage
        {
            Id = messageId,
            Payload = payload
        });
        await _context.SaveChangesAsync();
    }

    private async Task ProcessMessage(Message message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("RecieveNotification", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    private async Task MarkAsProcessed(Guid messageId)
    {
        var inboxMessage = await _context.InboxMessages.FirstOrDefaultAsync(x => x.Id == messageId);
        if (inboxMessage is not null)
        {
            inboxMessage.Processed = true;
            await _context.SaveChangesAsync();  
        }
    }
}