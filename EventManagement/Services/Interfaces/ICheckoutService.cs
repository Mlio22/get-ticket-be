using Common.DTO;
using EventManagement.DTO.Checkout;

namespace EventManagement.Services.Interfaces;

public interface ICheckoutService
{
    Task<DataResponse<CheckoutResponse>> CreateAsync(
        CreateCheckoutRequest request,
        CheckoutUserContext userContext,
        CancellationToken cancellationToken
    );
    Task<DataResponse<CheckoutResponse>> GetByIdAsync(Guid checkoutId, Guid userId);
    Task<DataResponse<AttendeeDashboardResponse>> GetAttendeeDashboardAsync(Guid userId);
    Task<DataResponse<List<OrderListItemResponse>>> GetMyOrdersAsync(Guid userId);
    Task<DataResponse<OrderListItemResponse>> GetMyOrderByIdAsync(Guid userId, string orderId);
    Task<DataResponse<List<OwnedTicketResponse>>> GetMyOwnedTicketsAsync(Guid userId);
    Task<DataResponse<List<OwnedTicketResponse>>> GetMyOwnedTicketsByEventAsync(
        Guid userId,
        Guid eventId
    );
    Task<BaseResponse> MarkTicketAsUsedAsync(Guid ticketId, Guid organizerId);
    Task<string> GetInvoiceUrlAsync(Guid checkoutId, Guid userId);
    Task<BaseResponse> HandleWebhookAsync(
        XenditInvoiceWebhookRequest request,
        string? callbackToken
    );
    Task ProcessExpiredCheckoutsAsync(CancellationToken cancellationToken);
}
