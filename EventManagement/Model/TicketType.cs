using Common.Model;
using Dapper.Contrib.Extensions;
using EventManagement.Enums;

namespace EventManagement.Model;

[Table("ticket_types")]
public class TicketType : BaseModel<Guid>
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "IDR";
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public DateTime? SaleStartDate { get; set; }
    public DateTime? SaleEndDate { get; set; }
    public TicketStatusEnum Status { get; set; } = TicketStatusEnum.Available;
    public bool IsDeleted { get; set; } = false;
}
