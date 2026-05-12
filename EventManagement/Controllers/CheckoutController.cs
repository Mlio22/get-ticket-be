using Common.DTO;
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
public class CheckoutController : AuthenticatedControllerBase
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
            GetCheckoutUserContext(),
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
}
