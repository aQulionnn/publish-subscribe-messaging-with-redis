using System.Text.Json;
using AutoMapper;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using Publisher.Dtos;
using Publisher.Entities;
using StackExchange.Redis;

namespace Publisher.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ActivityController(IConnectionMultiplexer redis, AppDbContext context, IMapper mapper)
    {
        _redis = redis;
        _context = context;
        _mapper = mapper;
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
    public IActionResult Update([FromRoute] int id, [FromBody] UpdateActivityDto updateActivityDto)
    {
        var updatedActivity = _mapper.Map<Activity>(updateActivityDto);
        updatedActivity.Id = id;
        _context.Activities.Update(updatedActivity);
        _context.SaveChanges();

        var message = new Message(Guid.NewGuid(), DateTime.UtcNow);
        string jsonMessage = JsonSerializer.Serialize(message);
        var subscriber = _redis.GetSubscriber();
        subscriber.Publish("activity", jsonMessage);
        
        return Ok("Activity updated");
    }

    [HttpGet]
    public IActionResult Get()
    {
        var activities = _context.Activities.ToList();
        return Ok(activities);
    }
    
}