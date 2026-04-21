using Common.Enums;
using Microsoft.AspNetCore.Http;

namespace Common.Security;

/// <summary>
/// Reads the current user's identity from incoming request headers injected by the
/// API gateway or Auth service. Headers: X-Username, X-Email, X-User-Id, X-Role-Id.
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Username
    {
        get
        {
            const string sys = "System";
            try
            {
                string username = ExtractHeader(HeaderName.UsernameHeader);
                return string.IsNullOrEmpty(username) ? sys : username;
            }
            catch
            {
                return sys;
            }
        }
    }

    public string Email
    {
        get
        {
            try
            {
                return ExtractHeader(HeaderName.EmailHeader);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public Guid UserId
    {
        get
        {
            try
            {
                string id = ExtractHeader(HeaderName.UserIdHeader);
                return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }

    public int RoleId
    {
        get
        {
            try
            {
                string roleIdString = ExtractHeader(HeaderName.RoleIdHeader);
                return string.IsNullOrEmpty(roleIdString) ? 0 : Convert.ToInt32(roleIdString);
            }
            catch
            {
                return 0;
            }
        }
    }

    public UserRole Role => (UserRole)RoleId;

    // ─────────────────────────────────────────────────────────────

    private string ExtractHeader(string headerName)
    {
        return _httpContextAccessor.HttpContext?.Request.Headers[headerName].FirstOrDefault()
            ?? string.Empty;
    }
}
