using Common.DB;
using Common.Utils;
using EventManagement.Enums;
using EventManagement.Model;
using EventManagement.Repositories.Interfaces;

namespace EventManagement.Repositories;

public class CheckoutRepository : ICheckoutRepository
{
    private readonly IDbManager _dbManager;

    public CheckoutRepository(IDbManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<int> CreateAsync(
        CheckoutSession checkoutSession,
        IReadOnlyCollection<CheckoutSessionItem> checkoutItems
    )
    {
        using var ctx = _dbManager.CreateContext();
        await ctx.BeginTransactionAsync();

        const string checkoutSql = """
            INSERT INTO checkout_sessions (
                id, invoice_external_id, xendit_invoice_id, xendit_invoice_url,
                user_id, user_email, user_full_name,
                event_id, quantity, total_amount, currency,
                payment_provider, payment_method, status, failure_reason,
                expires_at, paid_at, created_on, created_by, updated_on, updated_by
            )
            VALUES (
                @Id, @InvoiceExternalId, @XenditInvoiceId, @XenditInvoiceUrl,
                @UserId, @UserEmail, @UserFullName,
                @EventId, @Quantity, @TotalAmount, @Currency,
                @PaymentProvider, @PaymentMethod, @Status, @FailureReason,
                @ExpiresAt, @PaidAt, @CreatedOn, @CreatedBy, @UpdatedOn, @UpdatedBy
            )
            """;

        const string itemSql = """
            INSERT INTO checkout_session_items (
                id, checkout_id, ticket_type_id, ticket_name, quantity,
                unit_price, line_total, currency, created_on, created_by
            )
            VALUES (
                @Id, @CheckoutId, @TicketTypeId, @TicketName, @Quantity,
                @UnitPrice, @LineTotal, @Currency, @CreatedOn, @CreatedBy
            )
            """;

        var affectedRows = await Crud.InsertAsync(ctx, checkoutSql, checkoutSession);

        foreach (var checkoutItem in checkoutItems)
            affectedRows += await Crud.InsertAsync(ctx, itemSql, checkoutItem);

        await ctx.CommitAsync();
        return affectedRows;
    }

    public async Task<CheckoutSession?> GetByIdAsync(Guid id)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, invoice_external_id, xendit_invoice_id, xendit_invoice_url,
                   user_id, user_email, user_full_name,
                 event_id, quantity, total_amount, currency,
                   payment_provider, payment_method, status, failure_reason,
                   expires_at, paid_at, created_on, created_by, updated_on, updated_by
            FROM checkout_sessions
            WHERE id = @Id
            """;

        return await Crud.QueryFirstOrDefaultAsync<CheckoutSession>(ctx, sql, new { Id = id });
    }

    public async Task<CheckoutSession?> GetByInvoiceExternalIdAsync(string externalId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, invoice_external_id, xendit_invoice_id, xendit_invoice_url,
                   user_id, user_email, user_full_name,
                 event_id, quantity, total_amount, currency,
                   payment_provider, payment_method, status, failure_reason,
                   expires_at, paid_at, created_on, created_by, updated_on, updated_by
            FROM checkout_sessions
            WHERE invoice_external_id = @ExternalId
            """;

        return await Crud.QueryFirstOrDefaultAsync<CheckoutSession>(
            ctx,
            sql,
            new { ExternalId = externalId }
        );
    }

    public async Task<IReadOnlyList<CheckoutSession>> GetByUserIdAsync(Guid userId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, invoice_external_id, xendit_invoice_id, xendit_invoice_url,
                   user_id, user_email, user_full_name,
                 event_id, quantity, total_amount, currency,
                   payment_provider, payment_method, status, failure_reason,
                   expires_at, paid_at, created_on, created_by, updated_on, updated_by
            FROM checkout_sessions
            WHERE user_id = @UserId
            ORDER BY created_on DESC
            """;

        return (await Crud.QueryAsync<CheckoutSession>(ctx, sql, new { UserId = userId })).ToList();
    }

    public async Task<IReadOnlyList<CheckoutSessionItem>> GetItemsByCheckoutIdAsync(Guid checkoutId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, checkout_id, ticket_type_id, ticket_name, quantity,
                   unit_price, line_total, currency, created_on, created_by
            FROM checkout_session_items
            WHERE checkout_id = @CheckoutId
            ORDER BY ticket_name ASC
            """;

        return (
            await Crud.QueryAsync<CheckoutSessionItem>(ctx, sql, new { CheckoutId = checkoutId })
        ).ToList();
    }

    public async Task<int> AttachInvoiceAsync(
        Guid id,
        string invoiceId,
        string invoiceUrl,
        DateTime updatedOn,
        string updatedBy
    )
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE checkout_sessions
            SET xendit_invoice_id = @InvoiceId,
                xendit_invoice_url = @InvoiceUrl,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id
            """;

        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                InvoiceId = invoiceId,
                InvoiceUrl = invoiceUrl,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
            }
        );
    }

    public async Task<int> MarkFailedAsync(
        Guid id,
        string reason,
        DateTime updatedOn,
        string updatedBy
    )
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE checkout_sessions
            SET status = @Status,
                failure_reason = @Reason,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id AND status = @PendingStatus
            """;

        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                Reason = reason,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                Status = CheckoutStatusEnum.Failed,
                PendingStatus = CheckoutStatusEnum.Pending,
            }
        );
    }

    public async Task<int> MarkExpiredAsync(
        Guid id,
        string reason,
        DateTime updatedOn,
        string updatedBy
    )
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            UPDATE checkout_sessions
            SET status = @Status,
                failure_reason = @Reason,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id AND status = @PendingStatus
            """;

        return await Crud.UpdateAsync(
            ctx,
            sql,
            new
            {
                Id = id,
                Reason = reason,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                Status = CheckoutStatusEnum.Expired,
                PendingStatus = CheckoutStatusEnum.Pending,
            }
        );
    }

    public async Task<int> MarkPaidAsync(
        Guid id,
        string? xenditInvoiceId,
        string? paymentMethod,
        DateTime paidAt,
        DateTime updatedOn,
        string updatedBy
    )
    {
        using var ctx = _dbManager.CreateContext();
        await ctx.BeginTransactionAsync();

        const string countItemsSql = """
            SELECT COUNT(*)
            FROM checkout_session_items
            WHERE checkout_id = @Id
            """;

        var itemCount = await Crud.QueryFirstOrDefaultAsync<int>(
            ctx,
            countItemsSql,
            new { Id = id }
        );

        const string updateInventorySql = """
            UPDATE ticket_types AS tt
            SET available_seats = tt.available_seats - csi.quantity,
                status = CASE WHEN tt.available_seats - csi.quantity <= 0 THEN 2 ELSE tt.status END,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            FROM checkout_sessions AS cs
            INNER JOIN checkout_session_items AS csi ON csi.checkout_id = cs.id
            WHERE cs.id = @Id
              AND cs.status = @PendingStatus
              AND tt.id = csi.ticket_type_id
              AND tt.is_deleted = FALSE
              AND tt.available_seats >= csi.quantity
            """;

        var inventoryRows = await Crud.UpdateAsync(
            ctx,
            updateInventorySql,
            new
            {
                Id = id,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                PendingStatus = CheckoutStatusEnum.Pending,
            }
        );

        if (inventoryRows <= 0 || inventoryRows != itemCount)
        {
            await ctx.RollbackAsync();
            return 0;
        }

        const string updateCheckoutSql = """
            UPDATE checkout_sessions
            SET status = @PaidStatus,
                xendit_invoice_id = COALESCE(@XenditInvoiceId, xendit_invoice_id),
                payment_method = COALESCE(@PaymentMethod, payment_method),
                paid_at = @PaidAt,
                failure_reason = NULL,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id AND status = @PendingStatus
            """;

        var checkoutRows = await Crud.UpdateAsync(
            ctx,
            updateCheckoutSql,
            new
            {
                Id = id,
                XenditInvoiceId = xenditInvoiceId,
                PaymentMethod = paymentMethod,
                PaidAt = paidAt,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                PaidStatus = CheckoutStatusEnum.Paid,
                PendingStatus = CheckoutStatusEnum.Pending,
            }
        );

        if (checkoutRows <= 0)
        {
            await ctx.RollbackAsync();
            return 0;
        }

        const string insertTicketsSql = """
            WITH checkout_context AS (
                SELECT cs.id AS checkout_id,
                       cs.user_id,
                       cs.event_id,
                       COALESCE(cs.paid_at, cs.created_on) AS purchased_at,
                       ev.end_date AS expires_at
                FROM checkout_sessions AS cs
                INNER JOIN events AS ev ON ev.id = cs.event_id
                WHERE cs.id = @Id
            ),
            expanded_items AS (
                SELECT csi.ticket_type_id,
                       generate_series(1, csi.quantity) AS serial_no
                FROM checkout_session_items AS csi
                WHERE csi.checkout_id = @Id
            )
            INSERT INTO tickets (
                id,
                checkout_id,
                user_id,
                event_id,
                ticket_type_id,
                serial_no,
                qr_payload,
                status,
                purchased_at,
                expires_at,
                created_on,
                created_by
            )
            SELECT uuid_generate_v4(),
                   ctx.checkout_id,
                   ctx.user_id,
                   ctx.event_id,
                   item.ticket_type_id,
                   item.serial_no,
                   CONCAT('tkt:', ctx.checkout_id::text, ':', item.ticket_type_id::text, ':', item.serial_no::text),
                   @ActiveStatus,
                   ctx.purchased_at,
                   ctx.expires_at,
                   @UpdatedOn,
                   @UpdatedBy
            FROM checkout_context AS ctx
            CROSS JOIN expanded_items AS item
            """;

        var insertedTicketRows = await Crud.InsertAsync(
            ctx,
            insertTicketsSql,
            new
            {
                Id = id,
                UpdatedOn = updatedOn,
                UpdatedBy = updatedBy,
                ActiveStatus = TicketOwnershipStatusEnum.Active,
            }
        );

        if (insertedTicketRows <= 0)
        {
            await ctx.RollbackAsync();
            return 0;
        }

        await ctx.CommitAsync();
        return checkoutRows;
    }

    public async Task<List<CheckoutSession>> GetExpiredPendingAsync(DateTime now, int batchSize)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT id, invoice_external_id, xendit_invoice_id, xendit_invoice_url,
                   user_id, user_email, user_full_name,
                 event_id, quantity, total_amount, currency,
                   payment_provider, payment_method, status, failure_reason,
                   expires_at, paid_at, created_on, created_by, updated_on, updated_by
            FROM checkout_sessions
            WHERE status = @PendingStatus AND expires_at <= @Now
            ORDER BY expires_at ASC
            LIMIT @BatchSize
            """;

        return (
            await Crud.QueryAsync<CheckoutSession>(
                ctx,
                sql,
                new
                {
                    Now = now,
                    BatchSize = batchSize,
                    PendingStatus = CheckoutStatusEnum.Pending,
                }
            )
        ).ToList();
    }

    public async Task<int> GetActiveReservedQuantityAsync(Guid ticketTypeId, DateTime now)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT COALESCE(SUM(csi.quantity), 0)
                        FROM checkout_session_items AS csi
                        INNER JOIN checkout_sessions AS cs ON cs.id = csi.checkout_id
                        WHERE csi.ticket_type_id = @TicketTypeId
                            AND cs.status = @PendingStatus
                            AND cs.expires_at > @Now
            """;

        return await Crud.QueryFirstOrDefaultAsync<int>(
            ctx,
            sql,
            new
            {
                TicketTypeId = ticketTypeId,
                PendingStatus = CheckoutStatusEnum.Pending,
                Now = now,
            }
        );
    }

    public async Task<OrganizerSalesTotals> GetOrganizerSalesTotalsAsync(Guid organizerId)
    {
        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT
                COALESCE(COUNT(DISTINCT cs.user_id), 0) AS total_attendees,
                COALESCE(SUM(cs.quantity), 0) AS tickets_sold,
                COALESCE(SUM(cs.total_amount), 0) AS gross_revenue,
                COALESCE(MIN(cs.currency), 'IDR') AS currency
            FROM checkout_sessions AS cs
            INNER JOIN events AS ev ON ev.id = cs.event_id
            WHERE ev.organizer_id = @OrganizerId
              AND ev.is_deleted = FALSE
              AND cs.status = @PaidStatus
            """;

        return await Crud.QueryFirstOrDefaultAsync<OrganizerSalesTotals>(
                ctx,
                sql,
                new { OrganizerId = organizerId, PaidStatus = CheckoutStatusEnum.Paid }
            ) ?? new OrganizerSalesTotals();
    }

    public async Task<IReadOnlyList<OrganizerSalesByEvent>> GetSalesByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds
    )
    {
        if (eventIds.Count == 0)
            return [];

        using var ctx = _dbManager.CreateContext();
        const string sql = """
            SELECT
                cs.event_id,
                COALESCE(COUNT(DISTINCT cs.user_id), 0) AS attendee_count,
                COALESCE(SUM(cs.quantity), 0) AS sold_tickets,
                COALESCE(SUM(cs.total_amount), 0) AS gross_revenue,
                COALESCE(MIN(cs.currency), 'IDR') AS currency
            FROM checkout_sessions AS cs
            WHERE cs.status = @PaidStatus
              AND cs.event_id = ANY(@EventIds)
            GROUP BY cs.event_id
            """;

        return (
            await Crud.QueryAsync<OrganizerSalesByEvent>(
                ctx,
                sql,
                new { EventIds = eventIds.ToArray(), PaidStatus = CheckoutStatusEnum.Paid }
            )
        ).ToList();
    }
}
