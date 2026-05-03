using System.Security.Claims;
using Common.DTO;
using Common.Exceptions;
using EventManagement.DTO.Checkout;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Creates hosted checkout sessions backed by Xendit invoices.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>Create a new checkout session and reserve ticket slots in Redis.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(DataResponse<CheckoutResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _checkoutService.CreateAsync(
            request,
            GetUserContext(),
            cancellationToken
        );
        return Ok(result);
    }

    /// <summary>Get the latest state of a checkout session.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DataResponse<CheckoutResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var result = await _checkoutService.GetByIdAsync(id, userId);
        return Ok(result);
    }

    /// <summary>Get all orders for current user, including all statuses.</summary>
    [HttpGet("me/orders")]
    [ProducesResponseType(
        typeof(DataResponse<List<OrderListItemResponse>>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = GetUserId();
        var result = await _checkoutService.GetMyOrdersAsync(userId);
        return Ok(result);
    }

    /// <summary>Get owned tickets for current user derived from paid orders.</summary>
    [HttpGet("me/tickets")]
    [ProducesResponseType(typeof(DataResponse<List<OwnedTicketResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOwnedTickets()
    {
        var userId = GetUserId();
        var result = await _checkoutService.GetMyOwnedTicketsAsync(userId);
        return Ok(result);
    }

    /// <summary>Redirect directly to the hosted Xendit invoice page.</summary>
    [HttpGet("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RedirectToPay(Guid id)
    {
        var userId = GetUserId();
        var invoiceUrl = await _checkoutService.GetInvoiceUrlAsync(id, userId);
        return Redirect(invoiceUrl);
    }

    private CheckoutUserContext GetUserContext() =>
        new()
        {
            UserId = GetUserId(),
            Email =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email")
                ?? throw new UnauthorizedException("Email claim is missing."),
            FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Customer",
        };

    private Guid GetUserId()
    {
        var rawId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedException("User identifier claim is missing.");

        if (!Guid.TryParse(rawId, out var userId))
            throw new UnauthorizedException("User identifier claim is invalid.");

        return userId;
    }
}
