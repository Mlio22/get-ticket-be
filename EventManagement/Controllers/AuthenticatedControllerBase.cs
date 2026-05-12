using System.Security.Claims;
using Common.Exceptions;
using EventManagement.DTO.Checkout;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected CheckoutUserContext GetCheckoutUserContext() =>
        new()
        {
            UserId = GetUserId(),
            Email =
                User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email")
                ?? throw new UnauthorizedException("Email claim is missing."),
            FullName = User.FindFirstValue(ClaimTypes.Name) ?? "Customer",
        };

    protected Guid GetUserId()
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
