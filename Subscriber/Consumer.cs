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
            Message json;
            
            try
            {
                json = JsonSerializer.Deserialize<Message>(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid message format");
                return;
            }
            
            var messageId = json.Id;

            await using var transaction = await _context.Database.BeginTransactionAsync(stoppingToken);
            try
            {
                var isProcessed =
                    await _context.InboxMessages.AnyAsync(x => x.Id == messageId && x.Processed, stoppingToken);
                if (isProcessed) return;

                await _context.InboxMessages.AddAsync(new InboxMessage
                {
                    Id = messageId,
                    Payload = message
                }, stoppingToken);

                await _hubContext.Clients.All.SendAsync("RecieveNotification", json, stoppingToken);

                var inboxMessage = await _context.InboxMessages.FirstOrDefaultAsync(x => x.Id == messageId, stoppingToken);
                inboxMessage.Processed = true;

                await _context.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(stoppingToken);
                _logger.LogError(ex, "Error while processing inbox message");
                throw;
            }
        });
    }
}