using Auth.DTO.Auth;
using Auth.Services.Interfaces;
using Common.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Controllers;

/// <summary>Handles user authentication: login and registration.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Authenticate a user and receive a JWT bearer token.</summary>
    /// <param name="request">User credentials (email + password).</param>
    /// <returns>JWT token with user display information.</returns>
    /// <response code="200">Login successful. Returns JWT token, full name, and role.</response>
    /// <response code="400">Invalid credentials or request validation failure.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(DataResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get currently authenticated user profile.</summary>
    /// <returns>Logged-in user profile for the active JWT token.</returns>
    /// <response code="200">Returns current user profile.</response>
    /// <response code="401">Missing or invalid JWT token.</response>
    /// <response code="404">User from token does not exist.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(DataResponse<MeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Me()
    {
        var idClaim =
            User.FindFirst(
                "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            )?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(idClaim, out var userId))
        {
            return Unauthorized(
                new BaseResponse { IsOk = false, ErrorMessage = "Invalid user token." }
            );
        }

        var result = await _authService.GetMeAsync(userId);
        return result.IsOk ? Ok(result) : NotFound(result);
    }

    /// <summary>Register a new user account.</summary>
    /// <param name="request">Registration details: email, password, full name, and role.</param>
    /// <returns>Confirmation of the created account.</returns>
    /// <response code="200">Registration successful.</response>
    /// <response code="400">Validation failure or email already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsOk ? Ok(result) : BadRequest(result);
    }
}
