using Auth.Model;

namespace Auth.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<int> CreateAsync(User user);
}
