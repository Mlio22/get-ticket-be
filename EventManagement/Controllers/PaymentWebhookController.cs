using Common.DTO;
using EventManagement.DTO.Checkout;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Receives payment status callbacks from Xendit.</summary>
[ApiController]
[Route("api/webhooks/xendit")]
[AllowAnonymous]
[Produces("application/json")]
public class PaymentWebhookController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public PaymentWebhookController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    /// <summary>Handle Xendit invoice webhook callbacks.</summary>
    [HttpPost("invoices")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InvoiceWebhook([FromBody] XenditInvoiceWebhookRequest request)
    {
        var callbackToken = Request.Headers["x-callback-token"].FirstOrDefault();
        var result = await _checkoutService.HandleWebhookAsync(request, callbackToken);
        return Ok(result);
    }
}
