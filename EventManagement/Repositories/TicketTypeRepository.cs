using Common.DB;
using Common.DTO;
using Common.Utils;
using EventManagement.DTO.Event;
using EventManagement.Repositories.Interfaces;

namespace EventManagement.Repositories;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly IDbManager _dbManager;

    public TicketTypeRepository(IDbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<ListResponse<TicketTypeResponse>> GetByEventIdAsync(Guid eventId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, event_id, name, description, price, total_seats, available_seats, status
            FROM ticket_types
            WHERE event_id = @EventId AND is_deleted = FALSE
            ORDER BY price ASC
            """;
        var list = (
            await Crud.QueryAsync<TicketTypeResponse>(ctx, sql, new { EventId = eventId })
        ).ToList();
        return new ListResponse<TicketTypeResponse>
        {
            IsOk = true,
            List = list,
            RecordCount = list.Count,
        };
    }

    public async Task<TicketTypeResponse?> GetByIdAsync(Guid id)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, event_id, name, description, price, total_seats, available_seats, status
            FROM ticket_types
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await Crud.QueryFirstOrDefaultAsync<TicketTypeResponse>(ctx, sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Model.TicketType ticketType)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            INSERT INTO ticket_types (id, event_id, name, description, price, total_seats, available_seats, status, is_deleted, created_on, created_by)
            VALUES (@Id, @EventId, @Name, @Description, @Price, @TotalSeats, @AvailableSeats, @Status, @IsDeleted, @CreatedOn, @CreatedBy)
            """;
        return await Crud.InsertAsync(
            ctx,
            sql,
            new
            {
                ticketType.Id,
                ticketType.EventId,
                ticketType.Name,
                ticketType.Description,
                ticketType.Price,
                ticketType.TotalSeats,
                ticketType.AvailableSeats,
                ticketType.Status,
                ticketType.IsDeleted,
                ticketType.CreatedOn,
                ticketType.CreatedBy,
            }
        );
    }

    public async Task<int> UpdateAsync(Model.TicketType ticketType)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE ticket_types
            SET name = @Name, description = @Description, price = @Price,
                total_seats = @TotalSeats, available_seats = @AvailableSeats, status = @Status,
                updated_on = @UpdatedOn, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = FALSE
            """;
        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                ticketType.Name,
                ticketType.Description,
                ticketType.Price,
                ticketType.TotalSeats,
                ticketType.AvailableSeats,
                ticketType.Status,
                ticketType.UpdatedOn,
                ticketType.UpdatedBy,
                ticketType.Id,
            }
        );
    }

    public async Task<int> SoftDeleteAsync(Guid id, string updatedBy)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE ticket_types SET is_deleted = TRUE, updated_on = @UpdatedOn, updated_by = @UpdatedBy
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
