using System.Security.Claims;
using Common.DTO;
using EventManagement.DTO.Event;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Manages ticket types for events.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TicketTypeController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;

    public TicketTypeController(ITicketTypeService ticketTypeService)
    {
        _ticketTypeService = ticketTypeService;
    }

    /// <summary>Get all ticket types for a specific event (public).</summary>
    /// <param name="eventId">The event's unique identifier (GUID).</param>
    /// <returns>List of ticket types belonging to the event.</returns>
    /// <response code="200">Returns the list of ticket types for the event.</response>
    [HttpGet("event/{eventId:guid}")]
    [ProducesResponseType(typeof(ListResponse<TicketTypeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEvent(Guid eventId)
    {
        var result = await _ticketTypeService.GetByEventIdAsync(eventId);
        return Ok(result);
    }

    /// <summary>Get a single ticket type by its ID (public).</summary>
    /// <param name="id">The ticket type's unique identifier (GUID).</param>
    /// <returns>The matching ticket type.</returns>
    /// <response code="200">Ticket type found and returned.</response>
    /// <response code="404">No ticket type exists with the given ID.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DataResponse<TicketTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _ticketTypeService.GetByIdAsync(id);
        return result.IsOk ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a ticket type for an event. Requires <c>EventOrganizer</c> role.</summary>
    /// <param name="request">Ticket type details including event ID, name, price, and seat count.</param>
    /// <returns>The newly created ticket type.</returns>
    /// <response code="200">Ticket type created successfully.</response>
    /// <response code="400">Validation failure or referenced event not found.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="403">Authenticated user does not have the EventOrganizer role.</response>
    [HttpPost]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(DataResponse<TicketTypeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateTicketTypeRequest request)
    {
        var createdBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _ticketTypeService.CreateAsync(request, createdBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Soft-delete a ticket type. Requires <c>EventOrganizer</c> role.</summary>
    /// <param name="id">The ticket type's unique identifier (GUID).</param>
    /// <returns>Confirmation of the deletion.</returns>
    /// <response code="200">Ticket type deleted successfully.</response>
    /// <response code="400">Ticket type not found or not owned by the requestor.</response>
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
        var updatedBy = User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
        var result = await _ticketTypeService.DeleteAsync(id, updatedBy);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }
}
