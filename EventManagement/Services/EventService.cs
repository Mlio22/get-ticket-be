using Common.DTO;
using EventManagement.DTO.Event;
using EventManagement.Enums;
using EventManagement.Repositories.Interfaces;
using EventManagement.Services.Interfaces;

namespace EventManagement.Services;

public class EventService : IEventService
{
    private const string PlaceholderPosterImage = "https://placehold.co/600x900?text=Event+Poster";

    private readonly IEventRepository _eventRepository;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly IUserLookupRepository _userLookup;

    public EventService(
        IEventRepository eventRepository,
        ITicketTypeRepository ticketTypeRepository,
        IUserLookupRepository userLookup
    )
    {
        _eventRepository = eventRepository;
        _ticketTypeRepository = ticketTypeRepository;
        _userLookup = userLookup;
    }

    public async Task<ListResponse<EventResponse>> GetAllEventsAsync(
        bool includeUnpublished = false
    )
    {
        var result = await _eventRepository.GetAllAsync(includeUnpublished);
        await EnrichEventListAsync(result.List);
        return result;
    }

    public async Task<ListResponse<EventResponse>> GetMyEventsAsync(Guid organizerId)
    {
        var result = await _eventRepository.GetByOrganizerIdAsync(organizerId);
        await EnrichEventListAsync(result.List);
        return result;
    }

    public async Task<DataResponse<OrganizerDashboardResponse>> GetOrganizerDashboardAsync(
        Guid organizerId
    )
    {
        var myEvents = await _eventRepository.GetByOrganizerIdAsync(organizerId);
        await EnrichEventListAsync(myEvents.List);

        var recentEvents = myEvents
            .List.OrderByDescending(x => x.StartDate)
            .Take(3)
            .Select(
                (ev, index) =>
                    new OrganizerRecentEventResponse
                    {
                        Id = ev.Id,
                        Title = ev.Title,
                        StartDate = ev.StartDate,
                        EndDate = ev.EndDate,
                        Location = ev.Location,
                        Status = ev.Status,
                        TotalTickets = ev.TicketTypes.Sum(tt => tt.TotalSeats),
                        SoldTickets = GetStaticSoldTickets(index),
                        GrossRevenue = GetStaticGrossRevenue(index),
                        PosterImage = ev.PosterImage,
                    }
            )
            .ToList();

        return new DataResponse<OrganizerDashboardResponse>
        {
            IsOk = true,
            Data = new OrganizerDashboardResponse
            {
                Summary = new OrganizerDashboardSummaryResponse
                {
                    TotalEvents = myEvents.RecordCount,
                    TotalAttendees = 1843,
                    TicketsSold = 3270,
                    GrossRevenue = 785000000,
                    Currency = "IDR",
                },
                RecentEvents = recentEvents,
            },
        };
    }

    public async Task<DataResponse<EventResponse>> GetEventByIdAsync(Guid id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        if (ev is null)
            return new DataResponse<EventResponse>
            {
                IsOk = false,
                ErrorMessage = "Event not found.",
            };

        await EnrichSingleEventAsync(ev);
        return new DataResponse<EventResponse> { IsOk = true, Data = ev };
    }

    public async Task<BaseResponse> CreateEventAsync(
        UpsertEventRequest request,
        Guid organizerId,
        string createdBy
    )
    {
        var eventPayload = request.Event;

        var ev = new Model.Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Title = eventPayload.Title,
            Description = eventPayload.Description,
            Category = eventPayload.Category,
            Location = eventPayload.Location,
            Address = eventPayload.Address,
            Timezone = eventPayload.Timezone,
            PosterImage = eventPayload.PosterImage,
            BannerImage = eventPayload.BannerImage,
            IsFeatured = eventPayload.IsFeatured,
            StartDate = eventPayload.StartDate,
            EndDate = eventPayload.EndDate,
            Status = EventStatusEnum.Draft,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        var eventRows = await _eventRepository.CreateAsync(ev);
        var ticketRows = await CreateTicketTypesAsync(ev.Id, request.TicketTypes, createdBy);

        var rows = eventRows + ticketRows;
        return new BaseResponse
        {
            IsOk = eventRows > 0,
            Message = eventRows > 0 ? "Event created successfully." : "Failed to create event.",
            AnyChange = rows,
        };
    }

    public async Task<BaseResponse> UpdateEventAsync(
        Guid eventId,
        UpsertEventRequest request,
        Guid organizerId,
        string updatedBy
    )
    {
        var existing = await _eventRepository.GetByIdAsync(eventId);
        if (existing is null)
            return new BaseResponse { IsOk = false, ErrorMessage = "Event not found." };

        var eventPayload = request.Event;

        var ev = new Model.Event
        {
            Id = eventId,
            OrganizerId = organizerId,
            Title = eventPayload.Title,
            Description = eventPayload.Description,
            Category = eventPayload.Category,
            Location = eventPayload.Location,
            Address = eventPayload.Address,
            Timezone = eventPayload.Timezone,
            PosterImage = eventPayload.PosterImage,
            BannerImage = eventPayload.BannerImage,
            IsFeatured = eventPayload.IsFeatured,
            StartDate = eventPayload.StartDate,
            EndDate = eventPayload.EndDate,
            UpdatedOn = DateTime.UtcNow,
            UpdatedBy = updatedBy,
        };

        var eventRows = await _eventRepository.UpdateAsync(ev);
        if (eventRows <= 0)
        {
            return new BaseResponse
            {
                IsOk = false,
                Message = "Failed to update event.",
                AnyChange = eventRows,
            };
        }

        var deleteRows = await ReplaceTicketTypesAsync(eventId, request.TicketTypes, updatedBy);
        var rows = eventRows + deleteRows;
        return new BaseResponse
        {
            IsOk = true,
            Message = "Event updated successfully.",
            AnyChange = rows,
        };
    }

    public async Task<BaseResponse> DeleteEventAsync(Guid id, Guid organizerId, string updatedBy)
    {
        var existing = await _eventRepository.GetByIdAsync(id);
        if (existing is null)
            return new BaseResponse { IsOk = false, ErrorMessage = "Event not found." };

        var rows = await _eventRepository.SoftDeleteAsync(id, updatedBy);
        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Event deleted." : "Failed to delete event.",
            AnyChange = rows,
        };
    }

    private async Task EnrichEventListAsync(List<EventResponse> events)
    {
        if (events.Count == 0)
            return;

        var organizerIds = events.Select(e => e.OrganizerId).Distinct();
        var organizers = await _userLookup.GetByIdsAsync(organizerIds);
        var eventIds = events.Select(e => e.Id).Distinct();
        var ticketTypes = await _ticketTypeRepository.GetByEventIdsAsync(eventIds);

        foreach (var ev in events)
        {
            ev.PosterImage = GetPosterOrPlaceholder(ev.PosterImage);

            if (organizers.TryGetValue(ev.OrganizerId, out var org))
                ev.Organizer = org;
            if (ticketTypes.TryGetValue(ev.Id, out var tickets))
                ev.TicketTypes = tickets;
        }
    }

    private async Task EnrichSingleEventAsync(EventResponse ev)
    {
        ev.PosterImage = GetPosterOrPlaceholder(ev.PosterImage);
        ev.Organizer = await _userLookup.GetByIdAsync(ev.OrganizerId);
        ev.TicketTypes = (await _ticketTypeRepository.GetByEventIdAsync(ev.Id)).List;
    }

    private static string GetPosterOrPlaceholder(string? posterImage) =>
        string.IsNullOrWhiteSpace(posterImage) ? PlaceholderPosterImage : posterImage;

    // Placeholder metrics until attendee/order domain exists.
    private static int GetStaticSoldTickets(int index) =>
        index switch
        {
            0 => 1346,
            1 => 1020,
            _ => 0,
        };

    // Placeholder metrics until attendee/order domain exists.
    private static decimal GetStaticGrossRevenue(int index) =>
        index switch
        {
            0 => 412000000,
            1 => 96500000,
            _ => 0,
        };

    private async Task<int> CreateTicketTypesAsync(
        Guid eventId,
        IReadOnlyCollection<EventTicketTypePayload> ticketTypes,
        string actor
    )
    {
        if (ticketTypes.Count == 0)
            return 0;

        var rows = 0;
        foreach (var ticket in ticketTypes)
        {
            var model = new Model.TicketType
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                Name = ticket.Name,
                Description = ticket.Description,
                Price = ticket.Price,
                Currency = ticket.Currency,
                TotalSeats = ticket.TotalSeats,
                AvailableSeats = ticket.AvailableSeats ?? ticket.TotalSeats,
                SaleStartDate = ticket.SaleStartDate,
                SaleEndDate = ticket.SaleEndDate,
                Status = TicketStatusEnum.Available,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = actor,
            };

            rows += await _ticketTypeRepository.CreateAsync(model);
        }

        return rows;
    }

    private async Task<int> ReplaceTicketTypesAsync(
        Guid eventId,
        IReadOnlyCollection<EventTicketTypePayload> ticketTypes,
        string actor
    )
    {
        var rows = 0;
        var existingTicketTypes = await _ticketTypeRepository.GetByEventIdAsync(eventId);
        foreach (var existing in existingTicketTypes.List)
            rows += await _ticketTypeRepository.SoftDeleteAsync(existing.Id, actor);

        rows += await CreateTicketTypesAsync(eventId, ticketTypes, actor);
        return rows;
    }

    private static EventStatusEnum ParseStatus(string status) =>
        status.ToLowerInvariant() switch
        {
            "draft" => EventStatusEnum.Draft,
            "published" => EventStatusEnum.Published,
            "ongoing" => EventStatusEnum.Ongoing,
            "completed" => EventStatusEnum.Completed,
            "cancelled" => EventStatusEnum.Cancelled,
            _ => EventStatusEnum.Draft,
        };
}
