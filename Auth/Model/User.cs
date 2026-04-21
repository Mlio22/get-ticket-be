using Common.Enums;
using Common.Model;
using Dapper.Contrib.Extensions;

namespace Auth.Model;

[Table("users")]
public class User : BaseModel<Guid>
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}
