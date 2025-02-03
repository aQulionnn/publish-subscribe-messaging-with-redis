using Subscriber;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<Consumer>();

var app = builder.Build();

app.Run();