using Dapper.Contrib.Extensions;

namespace EventManagement.Model;

[Table("checkout_session_items")]
public class CheckoutSessionItem
{
    public Guid Id { get; set; }
    public Guid CheckoutId { get; set; }
    public Guid TicketTypeId { get; set; }
    public string TicketName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string Currency { get; set; } = "IDR";
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}
