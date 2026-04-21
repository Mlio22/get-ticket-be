namespace Common.Security;

/// <summary>HTTP header names used to propagate user identity between services.</summary>
public static class HeaderName
{
    public const string UsernameHeader = "X-Username";
    public const string EmailHeader = "X-Email";
    public const string UserIdHeader = "X-User-Id";
    public const string RoleIdHeader = "X-Role-Id";
}
