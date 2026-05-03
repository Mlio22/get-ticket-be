namespace EventManagement.DTO.Event;

/// <summary>Organizer dashboard response.</summary>
public class OrganizerDashboardResponse
{
    /// <summary>Aggregated dashboard counters.</summary>
    public OrganizerDashboardSummaryResponse Summary { get; set; } = new();

    /// <summary>Most recent events owned by the organizer.</summary>
    public List<OrganizerRecentEventResponse> RecentEvents { get; set; } = [];
}

/// <summary>Top-level dashboard summary metrics.</summary>
public class OrganizerDashboardSummaryResponse
{
    /// <summary>Total number of events owned by organizer.</summary>
    public int TotalEvents { get; set; }

    /// <summary>Placeholder attendees metric until attendee model is implemented.</summary>
    public int TotalAttendees { get; set; }

    /// <summary>Placeholder sold tickets metric until transaction model is implemented.</summary>
    public int TicketsSold { get; set; }

    /// <summary>Placeholder gross revenue metric until transaction model is implemented.</summary>
    public decimal GrossRevenue { get; set; }

    /// <summary>Currency code for summary amounts.</summary>
    public string Currency { get; set; } = "IDR";
}

/// <summary>Recent event item for organizer dashboard.</summary>
public class OrganizerRecentEventResponse
{
    /// <summary>Event ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Event title.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Event start timestamp (UTC).</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Event end timestamp (UTC).</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Display location.</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Event status label.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Total seats across all ticket types.</summary>
    public int TotalTickets { get; set; }

    /// <summary>Placeholder sold ticket count until transaction model is implemented.</summary>
    public int SoldTickets { get; set; }

    /// <summary>Placeholder revenue until transaction model is implemented.</summary>
    public decimal GrossRevenue { get; set; }

    /// <summary>Poster image URL (falls back to placeholder when empty).</summary>
    public string PosterImage { get; set; } = string.Empty;
}
