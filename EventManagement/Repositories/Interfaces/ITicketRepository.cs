using EventManagement.Model;

namespace EventManagement.Repositories.Interfaces;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<Ticket>> GetByCheckoutIdAsync(Guid checkoutId);
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<int> MarkAsUsedAsync(Guid id, DateTime usedAt, string updatedBy);
    Task<int> MarkAsExpiredAsync(Guid id, DateTime updatedOn, string updatedBy);
}
