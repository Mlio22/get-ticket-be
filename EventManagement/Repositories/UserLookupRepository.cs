using Dapper;
using EventManagement.DTO.Event;
using EventManagement.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EventManagement.Repositories;

public class UserLookupRepository : IUserLookupRepository
{
    private readonly string _authConnectionString;

    public UserLookupRepository(IConfiguration configuration)
    {
        _authConnectionString =
            configuration.GetConnectionString("AuthConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'AuthConnection' is not configured."
            );
    }

    public async Task<OrganizerResponse?> GetByIdAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_authConnectionString);
        const string sql = """
            SELECT id, full_name AS name, email,
                   CASE role
                       WHEN 2 THEN 'organizer'
                       ELSE 'customer'
                   END AS role,
                   created_on AS created_at
            FROM users
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await conn.QueryFirstOrDefaultAsync<OrganizerResponse>(sql, new { Id = id });
    }

    public async Task<IReadOnlyDictionary<Guid, OrganizerResponse>> GetByIdsAsync(
        IEnumerable<Guid> ids
    )
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
            return new Dictionary<Guid, OrganizerResponse>();

        await using var conn = new NpgsqlConnection(_authConnectionString);
        const string sql = """
            SELECT id, full_name AS name, email,
                   CASE role
                       WHEN 2 THEN 'organizer'
                       ELSE 'customer'
                   END AS role,
                   created_on AS created_at
            FROM users
            WHERE id = ANY(@Ids) AND is_deleted = FALSE
            """;
        var rows = await conn.QueryAsync<OrganizerResponse>(sql, new { Ids = idList.ToArray() });
        return rows.ToDictionary(r => r.Id);
    }
}
