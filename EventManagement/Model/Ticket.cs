using Common.Model;
using Dapper.Contrib.Extensions;
using EventManagement.Enums;

namespace EventManagement.Model;

[Table("tickets")]
public class Ticket : BaseModel<Guid>
{
    public Guid CheckoutId { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public Guid TicketTypeId { get; set; }
    public int SerialNo { get; set; }
    public string QrPayload { get; set; } = string.Empty;
    public TicketOwnershipStatusEnum Status { get; set; } = TicketOwnershipStatusEnum.Active;
    public DateTime PurchasedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
}