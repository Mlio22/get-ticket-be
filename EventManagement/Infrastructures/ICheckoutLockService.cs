namespace EventManagement.Infrastructures;

public interface ICheckoutLockService
{
    Task EnsureTicketStockAsync(
        Guid ticketTypeId,
        int dbAvailableSeats,
        int activeReservedQuantity
    );
    Task<CheckoutReservationResult> ReserveAsync(
        Guid checkoutId,
        Guid ticketTypeId,
        int quantity,
        DateTime expiresAt
    );
    Task FinalizeReservationAsync(Guid checkoutId, Guid ticketTypeId);
    Task ReleaseReservationAsync(Guid checkoutId, Guid ticketTypeId, int quantity);
}

public class CheckoutReservationResult
{
    public bool IsReserved { get; set; }
    public int RemainingStock { get; set; }
}
