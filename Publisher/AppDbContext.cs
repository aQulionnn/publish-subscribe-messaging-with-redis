using Microsoft.EntityFrameworkCore;
using Publisher.Entities;

namespace Publisher;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Activity> Activities { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
}