using Common.DB;
using Common.DTO;
using Common.Utils;
using EventManagement.DTO.Event;
using EventManagement.Repositories.Interfaces;

namespace EventManagement.Repositories;

public class EventRepository : IEventRepository
{
    private readonly IDbManager _dbManager;

    public EventRepository(IDbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<ListResponse<EventResponse>> GetAllAsync(bool includeUnpublished = false)
    {
        using var ctx = _dbManager.CreateContext();
        var sql =
            """
                SELECT id, organizer_id, title, description, location, start_date, end_date, status
                FROM events
                WHERE is_deleted = FALSE
                """
            + (includeUnpublished ? "" : " AND status = 2")
            + """

                ORDER BY start_date ASC
                """;

        var list = (await Crud.QueryAsync<EventResponse>(ctx, sql)).ToList();
        return new ListResponse<EventResponse>
        {
            IsOk = true,
            List = list,
            RecordCount = list.Count,
        };
    }

    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, organizer_id, title, description, location, start_date, end_date, status
            FROM events
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await Crud.QueryFirstOrDefaultAsync<EventResponse>(ctx, sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Model.Event ev)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            INSERT INTO events (id, organizer_id, title, description, location, start_date, end_date, status, is_deleted, created_on, created_by)
            VALUES (@Id, @OrganizerId, @Title, @Description, @Location, @StartDate, @EndDate, @Status, @IsDeleted, @CreatedOn, @CreatedBy)
            """;
        return await Crud.InsertAsync(
            ctx,
            sql,
            new
            {
                ev.Id,
                ev.OrganizerId,
                ev.Title,
                ev.Description,
                ev.Location,
                ev.StartDate,
                ev.EndDate,
                ev.Status,
                ev.IsDeleted,
                ev.CreatedOn,
                ev.CreatedBy,
            }
        );
    }

    public async Task<int> UpdateAsync(Model.Event ev)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE events
            SET title = @Title, description = @Description, location = @Location,
                start_date = @StartDate, end_date = @EndDate, status = @Status,
                updated_on = @UpdatedOn, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                ev.Title,
                ev.Description,
                ev.Location,
                ev.StartDate,
                ev.EndDate,
                ev.Status,
                ev.UpdatedOn,
                ev.UpdatedBy,
                ev.Id,
            }
        );
    }

    public async Task<int> SoftDeleteAsync(Guid id, string updatedBy)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE events SET is_deleted = TRUE, updated_on = @UpdatedOn, updated_by = @UpdatedBy
            WHERE id = @Id
            """;
        return await Crud.DeleteAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = updatedBy,
            }
        );
    }
}
