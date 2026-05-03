namespace EventManagement.DTO.Event;

/// <summary>Fields required to create a new ticket type for an event.</summary>
public class CreateTicketTypeRequest
{
    /// <summary>ID of the event this ticket type belongs to.</summary>
    public Guid EventId { get; set; }

    /// <summary>Display name of the ticket tier (e.g. <c>VIP</c>, <c>General Admission</c>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of what this ticket type includes.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Price per ticket in the event's currency.</summary>
    public decimal Price { get; set; }

    /// <summary>ISO currency code (e.g. <c>IDR</c>, <c>USD</c>).</summary>
    public string Currency { get; set; } = "IDR";

    /// <summary>Total number of seats available for this ticket tier.</summary>
    public int TotalSeats { get; set; }

    /// <summary>UTC date and time when ticket sales begin.</summary>
    public DateTime? SaleStartDate { get; set; }

    /// <summary>UTC date and time when ticket sales end.</summary>
    public DateTime? SaleEndDate { get; set; }
}

/// <summary>Ticket type data returned by the API.</summary>
public class TicketTypeResponse
{
    /// <summary>Unique identifier of the ticket type.</summary>
    public Guid Id { get; set; }

    /// <summary>ID of the event this ticket type belongs to.</summary>
    public Guid EventId { get; set; }

    /// <summary>Display name of the ticket tier.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of what this ticket type includes.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Price per ticket.</summary>
    public decimal Price { get; set; }

    /// <summary>ISO currency code (e.g. <c>IDR</c>, <c>USD</c>).</summary>
    public string Currency { get; set; } = "IDR";

    /// <summary>Total number of seats for this tier.</summary>
    public int TotalSeats { get; set; }

    /// <summary>Remaining seats that can still be purchased.</summary>
    public int AvailableSeats { get; set; }

    /// <summary>UTC date and time when ticket sales begin.</summary>
    public DateTime? SaleStartDate { get; set; }

    /// <summary>UTC date and time when ticket sales end.</summary>
    public DateTime? SaleEndDate { get; set; }

    /// <summary>Human-readable status (e.g. <c>Available</c>, <c>SoldOut</c>).</summary>
    public string Status { get; set; } = string.Empty;
}
