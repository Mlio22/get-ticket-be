using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

/// <summary>Service liveness check.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>Returns the current health status of the EventManagement service.</summary>
    /// <returns>Service name, status, and server UTC timestamp.</returns>
    /// <response code="200">Service is healthy.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(
            new
            {
                Status = "Healthy",
                Service = "EventManagement",
                Timestamp = DateTime.UtcNow,
            }
        );
    }
}
