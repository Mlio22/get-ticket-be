using Common.Model;
using Dapper.Contrib.Extensions;
using EventManagement.Enums;

namespace EventManagement.Model;

[Table("checkout_sessions")]
public class CheckoutSession : BaseModel<Guid>
{
    public string InvoiceExternalId { get; set; } = string.Empty;
    public string? XenditInvoiceId { get; set; }
    public string? XenditInvoiceUrl { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public Guid EventId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "IDR";
    public string PaymentProvider { get; set; } = "xendit";
    public string? PaymentMethod { get; set; }
    public CheckoutStatusEnum Status { get; set; } = CheckoutStatusEnum.Pending;
    public string? FailureReason { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
