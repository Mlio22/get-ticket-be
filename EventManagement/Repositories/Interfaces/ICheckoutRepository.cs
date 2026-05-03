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
}
