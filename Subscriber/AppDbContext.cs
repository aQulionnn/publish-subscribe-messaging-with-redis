using Microsoft.EntityFrameworkCore;
using Subscriber.Entities;

namespace Subscriber;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<InboxMessage> InboxMessages { get; set; }
}