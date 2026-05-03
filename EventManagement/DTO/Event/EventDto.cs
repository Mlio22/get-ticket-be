using EventManagement.Enums;

namespace EventManagement.DTO.Event;

/// <summary>Event body inside create/update payload.</summary>
public class EventPayload
{
    /// <summary>Short, descriptive title of the event.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Full description of the event.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Event category (e.g. <c>music</c>, <c>sport</c>, <c>conference</c>).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Venue name or city (short display location).</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Full street address of the venue.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>IANA timezone identifier for the event (e.g. <c>Asia/Jakarta</c>).</summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>URL of the event poster image (portrait, ~600 px wide).</summary>
    public string PosterImage { get; set; } = string.Empty;

    /// <summary>URL of the event banner image (landscape, ~1200 px wide).</summary>
    public string BannerImage { get; set; } = string.Empty;

    /// <summary>Whether this event should be highlighted as featured.</summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>UTC date and time when the event starts.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>UTC date and time when the event ends.</summary>
    public DateTime EndDate { get; set; }
}

/// <summary>Ticket type body inside create/update payload.</summary>
public class EventTicketTypePayload
{
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

    /// <summary>Remaining number of seats (optional; defaults to <c>TotalSeats</c>).</summary>
    public int? AvailableSeats { get; set; }

    /// <summary>UTC date and time when ticket sales begin.</summary>
    public DateTime? SaleStartDate { get; set; }

    /// <summary>UTC date and time when ticket sales end.</summary>
    public DateTime? SaleEndDate { get; set; }
}

/// <summary>Payload for creating/updating event and ticket types together.</summary>
public class UpsertEventRequest
{
    /// <summary>Main event fields.</summary>
    public EventPayload Event { get; set; } = new();

    /// <summary>Ticket types that belong to the event.</summary>
    public List<EventTicketTypePayload> TicketTypes { get; set; } = [];
}

/// <summary>Organizer summary embedded inside an event response.</summary>
public class OrganizerResponse
{
    /// <summary>Organizer's user ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Organizer's display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Organizer's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Organizer's role label (always <c>organizer</c>).</summary>
    public string Role { get; set; } = "organizer";

    /// <summary>UTC timestamp when the organizer account was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Event data returned by the API.</summary>
public class EventResponse
{
    /// <summary>Unique identifier of the event.</summary>
    public Guid Id { get; set; }

    /// <summary>Unique identifier of the organizer who owns this event.</summary>
    public Guid OrganizerId { get; set; }

    /// <summary>Short, descriptive title of the event.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Full description of the event.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Event category (e.g. <c>music</c>, <c>sport</c>).</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Venue name or city (short display location).</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Full street address of the venue.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>IANA timezone identifier for the event (e.g. <c>Asia/Jakarta</c>).</summary>
    public string Timezone { get; set; } = "UTC";

    /// <summary>URL of the event poster image.</summary>
    public string PosterImage { get; set; } = string.Empty;

    /// <summary>URL of the event banner image.</summary>
    public string BannerImage { get; set; } = string.Empty;

    /// <summary>Whether this event is highlighted as featured.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>UTC date and time when the event starts.</summary>
    public DateTime StartDate { get; set; }

    /// <summary>UTC date and time when the event ends.</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Human-readable event status (e.g. <c>Draft</c>, <c>Published</c>).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the event record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when the event record was last updated.</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Organizer details. Populated on single-event lookups and list responses.</summary>
    public OrganizerResponse? Organizer { get; set; }

    /// <summary>Ticket types for this event.</summary>
    public List<TicketTypeResponse> TicketTypes { get; set; } = [];
}
