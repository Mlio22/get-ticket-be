using System.Security.Claims;
using EventManagement.DTO.Event;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>Get all published events (public).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _eventService.GetAllEventsAsync(includeUnpublished: false);
        return Ok(result);
    }

    /// <summary>Get a single event by ID (public).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        return result.IsOk ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new event (Event Organizer only).</summary>
    [HttpPost]
    [Authorize(Roles = "EventOrganizer")]
    public async Task<IActionResult> Create([FromBody] CreateEventRequest request)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.CreateEventAsync(request, organizerId, createdBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update an event (Event Organizer only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EventOrganizer")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        request.Id = id;
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.UpdateEventAsync(request, organizerId, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete an event (Event Organizer only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "EventOrganizer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.DeleteEventAsync(id, organizerId, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }
}
