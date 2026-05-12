using Common.DB;
using Common.Utils;
using EventManagement.Enums;
using EventManagement.Model;
using EventManagement.Repositories.Interfaces;

namespace EventManagement.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly IDbManager _dbManager;

    public TicketRepository(IDbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, checkout_id, user_id, event_id, ticket_type_id,
                   serial_no, qr_payload, status, purchased_at,
                   expires_at, used_at, created_on, created_by, updated_on, updated_by
            FROM tickets
            WHERE user_id = @UserId
            ORDER BY purchased_at DESC, created_on DESC
            """;

        return (await Crud.QueryAsync<Ticket>(ctx, sql, new { UserId = userId })).ToList();
    }

    public async Task<IReadOnlyList<Ticket>> GetByCheckoutIdAsync(Guid checkoutId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, checkout_id, user_id, event_id, ticket_type_id,
                   serial_no, qr_payload, status, purchased_at,
                   expires_at, used_at, created_on, created_by, updated_on, updated_by
            FROM tickets
            WHERE checkout_id = @CheckoutId
            ORDER BY serial_no ASC
            """;

        return (await Crud.QueryAsync<Ticket>(ctx, sql, new { CheckoutId = checkoutId })).ToList();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, checkout_id, user_id, event_id, ticket_type_id,
                   serial_no, qr_payload, status, purchased_at,
                   expires_at, used_at, created_on, created_by, updated_on, updated_by
            FROM tickets
            WHERE id = @Id
            """;

        return await Crud.QueryFirstOrDefaultAsync<Ticket>(ctx, sql, new { Id = id });
    }

    public async Task<int> MarkAsUsedAsync(Guid id, DateTime usedAt, string updatedBy)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE tickets
            SET status = @UsedStatus,
                used_at = @UsedAt,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id AND status = @ActiveStatus
            """;

        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                UsedAt = usedAt,
                UpdatedOn = usedAt,
                UpdatedBy = updatedBy,
                UsedStatus = TicketOwnershipStatusEnum.Used,
                ActiveStatus = TicketOwnershipStatusEnum.Active,
            }
        );
    }

    public async Task<int> MarkAsExpiredAsync(Guid id, DateTime updatedOn, string updatedBy)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE tickets
            SET status = @ExpiredStatus,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id AND status = @ActiveStatus
            """;

        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                ExpiredStatus = TicketOwnershipStatusEnum.Expired,
                ActiveStatus = TicketOwnershipStatusEnum.Active,
            }
        );
    }
}
