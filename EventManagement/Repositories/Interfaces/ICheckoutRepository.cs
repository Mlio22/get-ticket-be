using EventManagement.Model;

namespace EventManagement.Repositories.Interfaces;

public interface ICheckoutRepository
{
    Task<int> CreateAsync(
        CheckoutSession checkoutSession,
        IReadOnlyCollection<CheckoutSessionItem> checkoutItems
    );
    Task<CheckoutSession?> GetByIdAsync(Guid id);
    Task<CheckoutSession?> GetByInvoiceExternalIdAsync(string externalId);
    Task<IReadOnlyList<CheckoutSession>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<CheckoutSessionItem>> GetItemsByCheckoutIdAsync(Guid checkoutId);
    Task<int> AttachInvoiceAsync(
        Guid id,
        string invoiceId,
        string invoiceUrl,
        DateTime updatedOn,
        string updatedBy
    );
    Task<int> MarkFailedAsync(Guid id, string reason, DateTime updatedOn, string updatedBy);
    Task<int> MarkExpiredAsync(Guid id, string reason, DateTime updatedOn, string updatedBy);
    Task<int> MarkPaidAsync(
        Guid id,
        string? xenditInvoiceId,
        string? paymentMethod,
        DateTime paidAt,
        DateTime updatedOn,
        string updatedBy
    );
    Task<List<CheckoutSession>> GetExpiredPendingAsync(DateTime now, int batchSize);
    Task<int> GetActiveReservedQuantityAsync(Guid ticketTypeId, DateTime now);
    Task<OrganizerSalesTotals> GetOrganizerSalesTotalsAsync(Guid organizerId);
    Task<IReadOnlyList<OrganizerSalesByEvent>> GetSalesByEventIdsAsync(
        IReadOnlyCollection<Guid> eventIds
    );
}

public class OrganizerSalesTotals
{
    public int TotalAttendees { get; set; }
    public int TicketsSold { get; set; }
    public decimal GrossRevenue { get; set; }
    public string Currency { get; set; } = "IDR";
}

public class OrganizerSalesByEvent
{
    public Guid EventId { get; set; }
    public int AttendeeCount { get; set; }
    public int SoldTickets { get; set; }
    public decimal GrossRevenue { get; set; }
    public string Currency { get; set; } = "IDR";
}
