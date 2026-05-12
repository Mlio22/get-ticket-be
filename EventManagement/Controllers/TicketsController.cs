using Common.DTO;
using EventManagement.DTO.Checkout;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Provides owned ticket endpoints for authenticated attendees.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TicketsController : AuthenticatedControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public TicketsController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>Get owned tickets for current user derived from paid orders.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(DataResponse<List<OwnedTicketResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine()
    {
        var result = await _checkoutService.GetMyOwnedTicketsAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>Get owned tickets for current user for one event.</summary>
    [HttpGet("me/event/{eventId:guid}")]
    [ProducesResponseType(typeof(DataResponse<List<OwnedTicketResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMineByEvent(Guid eventId)
    {
        var result = await _checkoutService.GetMyOwnedTicketsByEventAsync(GetUserId(), eventId);
        return Ok(result);
    }

    /// <summary>Mark one ticket as used during attendee check-in.</summary>
    [HttpPatch("{id:guid}/use")]
    [Authorize(Roles = "EventOrganizer")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkUsed(Guid id)
    {
        var result = await _checkoutService.MarkTicketAsUsedAsync(id, GetUserId());
        return Ok(result);
    }
}
