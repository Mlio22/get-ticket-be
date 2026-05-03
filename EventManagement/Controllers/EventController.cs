using System.Security.Claims;
using Common.DTO;
using EventManagement.DTO.Event;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Manages event lifecycle operations.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>Get all published events (public).</summary>
    /// <returns>List of all published events.</returns>
    /// <response code="200">Returns a list of published events.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ListResponse<EventResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _eventService.GetAllEventsAsync(includeUnpublished: false);
        return Ok(result);
    }

    /// <summary>Get events owned by the logged-in Event Organizer.</summary>
    /// <returns>List of events owned by current organizer (including unpublished).</returns>
    /// <response code="200">Returns organizer-owned events.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpGet("mine")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(ListResponse<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMine()
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventService.GetMyEventsAsync(organizerId);
        return Ok(result);
    }

    /// <summary>Get dashboard data for the logged-in Event Organizer.</summary>
    /// <returns>Organizer summary metrics and recent events.</returns>
    /// <response code="200">Returns dashboard data for organizer.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpGet("dashboard")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(
        typeof(DataResponse<OrganizerDashboardResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDashboard()
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _eventService.GetOrganizerDashboardAsync(organizerId);
        return Ok(result);
    }

    /// <summary>Get a single published event by its ID (public).</summary>
    /// <param name="id">The event's unique identifier (GUID).</param>
    /// <returns>The matching event.</returns>
    /// <response code="200">Event found and returned.</response>
    /// <response code="404">No event exists with the given ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DataResponse<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _eventService.GetEventByIdAsync(id);
        return result.IsOk ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new event with ticket types. Requires <c>EventOrganizer</c> role.</summary>
    /// <param name="request">Payload with <c>event</c> object and <c>ticketTypes</c> array.</param>
    /// <returns>The newly created event.</returns>
    /// <response code="200">Event created successfully.</response>
    /// <response code="400">Validation failure.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpPost]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(DataResponse<EventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] UpsertEventRequest request)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.CreateEventAsync(request, organizerId, createdBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update an existing event with ticket types. Requires <c>EventOrganizer</c> role.</summary>
    /// <param name="id">The event's unique identifier (GUID).</param>
    /// <param name="request">Payload with <c>event</c> object and <c>ticketTypes</c> array.</param>
    /// <returns>Confirmation of the update.</returns>
    /// <response code="200">Event updated successfully.</response>
    /// <response code="400">Validation failure or event not owned by the requestor.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertEventRequest request)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.UpdateEventAsync(id, request, organizerId, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Soft-delete an event. Requires <c>EventOrganizer</c> role.</summary>
    /// <param name="id">The event's unique identifier (GUID).</param>
    /// <returns>Confirmation of the deletion.</returns>
    /// <response code="200">Event deleted successfully.</response>
    /// <response code="400">Event not found or not owned by the requestor.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var organizerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _eventService.DeleteEventAsync(id, organizerId, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }
}
