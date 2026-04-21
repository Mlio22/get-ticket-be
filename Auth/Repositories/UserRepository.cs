using Auth.Model;
using Auth.Repositories.Interfaces;
using Common.DB;
using Common.Utils;

namespace Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbManager _dbManager;

    public UserRepository(IDbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, email, password_hash, full_name, role, is_active, is_deleted,
                   created_on, created_by, updated_on, updated_by
            FROM users
            WHERE email = @Email AND is_deleted = FALSE
            """;
        return await Crud.QueryFirstOrDefaultAsync<User>(ctx, sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, email, password_hash, full_name, role, is_active, is_deleted,
                   created_on, created_by, updated_on, updated_by
            FROM users
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await Crud.QueryFirstOrDefaultAsync<User>(ctx, sql, new { Id = id });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            INSERT INTO users (id, email, password_hash, full_name, role, is_active, is_deleted, created_on, created_by)
            VALUES (@Id, @Email, @PasswordHash, @FullName, @Role, @IsActive, @IsDeleted, @CreatedOn, @CreatedBy)
            """;
        return await Crud.InsertAsync(
            ctx,
            sql,
            new
            {
                user.Id,
                user.Email,
                user.PasswordHash,
                user.FullName,
                user.Role,
                user.IsActive,
                user.IsDeleted,
                user.CreatedOn,
                user.CreatedBy,
            }
        );
    }
}
