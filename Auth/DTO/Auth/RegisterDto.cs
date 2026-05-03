using Common.Enums;

namespace Auth.DTO.Auth;

/// <summary>Details required to create a new user account.</summary>
public class RegisterRequest
{
    /// <summary>Unique email address for the new account.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Password for the new account.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>User's display name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Account role: <c>1 = Customer</c>, <c>2 = EventOrganizer</c>. Defaults to <c>Customer</c>.</summary>
    // public UserRole Role { get; set; } = UserRole.Customer;
}
