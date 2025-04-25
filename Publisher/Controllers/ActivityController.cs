using System.Text.Json;
using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Publisher.Dtos;
using Publisher.Entities;
using Publisher.Utilities;
using StackExchange.Redis;

namespace Publisher.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly Kernel _kernel;

    public ActivityController(IConnectionMultiplexer redis, AppDbContext context, IMapper mapper, Kernel kernel)
    {
        _redis = redis;
        _context = context;
        _mapper = mapper;
        _kernel = kernel;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateActivityDto createActivityDto)
    {
        var activity = _mapper.Map<Activity>(createActivityDto);
        _context.Activities.Add(activity);
        _context.SaveChanges();
        
        return Ok("Activity created");
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateActivityDto updateActivityDto)
    {
        var activity = await _context.Activities.FirstOrDefaultAsync(a => a.Id == id);
        if (activity == null) 
            return NotFound("Activity not found");
        
        var originalActivity = JsonSerializer.Serialize(_mapper.Map<UpdateActivityDto>(activity));
        
        _mapper.Map(updateActivityDto, activity);
        await _context.SaveChangesAsync();

        var prompt = $"compare these 2 object and write down what has been changed in short 2 sentences: from {originalActivity}, to {JsonSerializer.Serialize(updateActivityDto)}";
        var summarize = _kernel.CreateFunctionFromPrompt(prompt, executionSettings: new OpenAIPromptExecutionSettings {MaxTokens = 100});
        var completion = await _kernel.InvokeAsync(summarize);
        
        var message = new Message(Guid.NewGuid(), DateTime.UtcNow, completion.ToString());
        string jsonMessage = JsonSerializer.Serialize(message);

        var outbox = new OutboxMessage
        {
            Id = MessageIdGenerator.Create(jsonMessage),
            CreatedAt = DateTime.UtcNow,
            Type = "activity",
            Payload = jsonMessage
        };
        await _context.OutboxMessages.AddAsync(outbox);
        await _context.SaveChangesAsync();
        
        return Ok("Activity updated");
    }

    [HttpGet]
    public IActionResult Get()
    {
        var activities = _context.Activities.ToList();
        return Ok(activities);
    }
}