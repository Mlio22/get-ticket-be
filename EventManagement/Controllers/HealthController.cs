using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>Health check endpoint for monitoring and load balancing.</summary>
    [HttpGet]
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
