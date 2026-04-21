namespace EventManagement.DTO.Event;

public class CreateTicketTypeRequest
{
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
}

public class TicketTypeResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string Status { get; set; } = string.Empty;
}
