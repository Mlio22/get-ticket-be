using System.Text.Json.Serialization;
using EventManagement.DTO.Event;

namespace EventManagement.DTO.Checkout;

/// <summary>Single ticket line requested by the UI checkout page.</summary>
public class CreateCheckoutItemRequest
{
    /// <summary>Ticket type to purchase.</summary>
    public Guid TicketTypeId { get; set; }

    /// <summary>Number of tickets to reserve for this ticket type.</summary>
    public int Quantity { get; set; }
}

/// <summary>Request to create a hosted checkout session for one event and multiple ticket types.</summary>
public class CreateCheckoutRequest
{
    /// <summary>Event being checked out.</summary>
    public Guid EventId { get; set; }

    /// <summary>Cart line items grouped under one Xendit invoice.</summary>
    public List<CreateCheckoutItemRequest> Items { get; set; } = [];

    /// <summary>Optional URL to send user after successful payment.</summary>
    public string? SuccessRedirectUrl { get; set; }

    /// <summary>Optional URL to send user after failed or cancelled payment.</summary>
    public string? FailureRedirectUrl { get; set; }
}

/// <summary>Hosted checkout session details.</summary>
public class CheckoutResponse
{
    /// <summary>Local checkout session ID.</summary>
    public Guid CheckoutId { get; set; }

    /// <summary>External invoice identifier sent to Xendit.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Xendit hosted invoice URL.</summary>
    public string InvoiceUrl { get; set; } = string.Empty;

    /// <summary>Convenience redirect path in this API.</summary>
    public string PaymentPagePath { get; set; } = string.Empty;

    /// <summary>Current checkout status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Event title.</summary>
    public string EventTitle { get; set; } = string.Empty;

    /// <summary>Event ID associated with this checkout.</summary>
    public Guid EventId { get; set; }

    /// <summary>Reserved ticket quantity across all items.</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Total checkout amount.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Checkout currency.</summary>
    public string Currency { get; set; } = "IDR";

    /// <summary>When the Redis hold and invoice expire.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Payment completion timestamp, if available.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Failure message for expired or failed checkouts.</summary>
    public string? FailureReason { get; set; }

    /// <summary>All reserved ticket items in this checkout.</summary>
    public List<CheckoutItemResponse> Items { get; set; } = [];
}

/// <summary>Single line item returned for a checkout session.</summary>
public class CheckoutItemResponse
{
    /// <summary>Ticket type ID.</summary>
    public Guid TicketTypeId { get; set; }

    /// <summary>Ticket type display name.</summary>
    public string TicketName { get; set; } = string.Empty;

    /// <summary>Reserved quantity for this line item.</summary>
    public int Quantity { get; set; }

    /// <summary>Unit price for this ticket type.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Line total for this ticket type.</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Currency used for this line item.</summary>
    public string Currency { get; set; } = "IDR";
}

/// <summary>Order entry returned on the current user's order list endpoint.</summary>
public class OrderListItemResponse
{
    /// <summary>Order identifier in API response.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Checkout session ID.</summary>
    public Guid CheckoutId { get; set; }

    /// <summary>External invoice identifier sent to Xendit.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>User ID owning this order.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Current order status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Event details for this order.</summary>
    public EventResponse? Event { get; set; }

    /// <summary>Invoice URL from Xendit, when available.</summary>
    public string InvoiceUrl { get; set; } = string.Empty;

    /// <summary>Payment method reported by webhook.</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Total quantity in this order.</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Total amount in this order.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Order currency.</summary>
    public string Currency { get; set; } = "IDR";

    /// <summary>Creation timestamp of this order.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Expiration timestamp for pending order.</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Payment completion timestamp for paid order.</summary>
    public DateTime? PaidAt { get; set; }

    /// <summary>Failure reason for failed/expired/cancelled order.</summary>
    public string? FailureReason { get; set; }

    /// <summary>Purchased items for this order.</summary>
    public List<CheckoutItemResponse> Items { get; set; } = [];
}

/// <summary>Owned ticket entry returned for paid orders.</summary>
public class OwnedTicketResponse
{
    /// <summary>Ticket identifier in API response.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Ticket type identifier.</summary>
    public string TicketTypeId { get; set; } = string.Empty;

    /// <summary>Ticket type details.</summary>
    public TicketTypeResponse? TicketType { get; set; }

    /// <summary>User ID owning this ticket.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Order identifier in API response.</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Event details for this ticket.</summary>
    public EventResponse? Event { get; set; }

    /// <summary>QR payload string for ticket check-in.</summary>
    public string QrCode { get; set; } = string.Empty;

    /// <summary>Current ticket status.</summary>
    public string Status { get; set; } = "active";

    /// <summary>Purchase timestamp.</summary>
    public DateTime PurchasedAt { get; set; }
}

/// <summary>Authenticated customer information used during checkout.</summary>
public class CheckoutUserContext
{
    /// <summary>User ID from JWT.</summary>
    public Guid UserId { get; set; }

    /// <summary>Customer email from JWT.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Customer display name from JWT.</summary>
    public string FullName { get; set; } = string.Empty;
}

/// <summary>Xendit invoice webhook payload.</summary>
public class XenditInvoiceWebhookRequest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("invoice_url")]
    public string? InvoiceUrl { get; set; }

    [JsonPropertyName("payment_method")]
    public string? PaymentMethod { get; set; }

    [JsonPropertyName("paid_at")]
    public DateTime? PaidAt { get; set; }

    [JsonPropertyName("failure_reason")]
    public string? FailureReason { get; set; }
}
