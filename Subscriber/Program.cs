using Microsoft.EntityFrameworkCore;
using Subscriber;
using Subscriber.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Database"));

builder.Services.AddSignalR();

builder.Services.AddHostedService<Consumer>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("react", builder =>
    {
        builder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("react");

app.MapHub<NotificationHub>("notification");

app.Run();