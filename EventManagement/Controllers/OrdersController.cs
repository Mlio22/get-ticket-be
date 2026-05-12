using Common.DTO;
using EventManagement.DTO.Checkout;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Provides order history endpoints for authenticated attendees.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : AuthenticatedControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public OrdersController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>Get all orders for current user, including all statuses.</summary>
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(DataResponse<List<OrderListItemResponse>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine()
    {
        var result = await _checkoutService.GetMyOrdersAsync(GetUserId());
        return Ok(result);
    }

    /// <summary>Get one order detail for current user by order id (ord-...).</summary>
    [HttpGet("me/{id}")]
    [ProducesResponseType(typeof(DataResponse<OrderListItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMineById(string id)
    {
        var result = await _checkoutService.GetMyOrderByIdAsync(GetUserId(), id);
        return Ok(result);
    }
}
