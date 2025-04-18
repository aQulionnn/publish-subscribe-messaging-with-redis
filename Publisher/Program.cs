using DotNetEnv;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Publisher;
using Publisher.BackgroundTasks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(options => options.UseInMemoryStorage());
builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));
builder.Services.AddTransient<OutboxPublisherJob>();

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("Database"));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddSingleton(sp =>
{
    Env.Load();
    
    var kernelBuilder = Kernel.CreateBuilder()
        .AddAzureOpenAIChatCompletion(
            Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")!,
            Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")!,
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")!
        );
    
    return kernelBuilder.Build();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseSwagger();
    app.UseHangfireDashboard();
}

app.Services
    .GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<OutboxPublisherJob>("publish-outbox-messages", job => job.PublishOutboxMessagesAsync(), "0/15 * * * * *");

app.MapControllers();

app.Run();