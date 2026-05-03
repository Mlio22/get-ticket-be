namespace Auth.DTO.Auth;

/// <summary>Credentials used to authenticate an existing user.</summary>
public class LoginRequest
{
    /// <summary>Registered email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Account password.</summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>Successful login payload.</summary>
public class LoginResponse
{
    /// <summary>Signed JWT bearer token. Include as <c>Authorization: Bearer {token}</c> on subsequent requests.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Authenticated user's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Authenticated user's role (e.g. <c>Customer</c>, <c>EventOrganizer</c>).</summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>Authenticated user profile returned by <c>/api/auth/me</c>.</summary>
public class MeResponse
{
    /// <summary>User ID as a string.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>User email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Role label for frontend: <c>user</c>, <c>organizer</c>, or <c>admin</c>.</summary>
    public string Role { get; set; } = "user";

    /// <summary>Optional avatar URL.</summary>
    public string? Avatar { get; set; }

    /// <summary>Optional phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>UTC timestamp when the account was created.</summary>
    public DateTime CreatedAt { get; set; }
}
