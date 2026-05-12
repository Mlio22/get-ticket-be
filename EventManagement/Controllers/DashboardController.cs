using Common.DTO;
using EventManagement.DTO.Checkout;
using EventManagement.DTO.Event;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Provides dashboard endpoints for attendees and event organizers.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : AuthenticatedControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly IEventService _eventService;

    public DashboardController(ICheckoutService checkoutService, IEventService eventService)
    {
        _checkoutService = checkoutService;
        _eventService = eventService;
    }

    /// <summary>Get attendee dashboard summary for current authenticated user.</summary>
    [HttpGet("attendee")]
    [ProducesResponseType(typeof(DataResponse<AttendeeDashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAttendeeDashboard()
    {
        var result = await _checkoutService.GetAttendeeDashboardAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>Get organizer dashboard summary for current authenticated organizer.</summary>
    [HttpGet("organizer")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(
        typeof(DataResponse<OrganizerDashboardResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganizerDashboard()
    {
        var result = await _eventService.GetOrganizerDashboardAsync(GetUserId());
        return Ok(result);
    }
}
