using System.Security.Claims;
using EventManagement.DTO.Event;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketTypeController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypeController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

    /// <summary>Get all ticket types for an event (public).</summary>
    [HttpGet("event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId)
    {
        var result = await _ticketTypeService.GetByEventIdAsync(eventId);
        return Ok(result);
    }

    /// <summary>Get a single ticket type by ID (public).</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _ticketTypeService.GetByIdAsync(id);
        return result.IsOk ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a ticket type for an event (Event Organizer only).</summary>
    [HttpPost]
    [Authorize(Roles = "EventOrganizer")]
    public async Task<IActionResult> Create([FromBody] CreateTicketTypeRequest request)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _ticketTypeService.CreateAsync(request, createdBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a ticket type (Event Organizer only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "EventOrganizer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _ticketTypeService.DeleteAsync(id, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }
}
