using Microsoft.EntityFrameworkCore;
using Publisher.Entities;

namespace Publisher;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Activity> Activities { get; set; }
}