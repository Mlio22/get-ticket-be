using Common.Enums;

namespace Common.Security;

public interface ICurrentUser
{
    string Username { get; }
    string Email { get; }
    int RoleId { get; }
    UserRole Role { get; }
    Guid UserId { get; }
}
