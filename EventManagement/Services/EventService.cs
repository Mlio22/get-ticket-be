using Common.DTO;
using EventManagement.DTO.Event;
using EventManagement.Enums;
using EventManagement.Repositories.Interfaces;
using EventManagement.Services.Interfaces;

namespace EventManagement.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public Task<ListResponse<EventResponse>> GetAllEventsAsync(bool includeUnpublished = false) =>
        _eventRepository.GetAllAsync(includeUnpublished);

    public async Task<DataResponse<EventResponse>> GetEventByIdAsync(Guid id)
    {
        var ev = await _eventRepository.GetByIdAsync(id);
        if (ev is null)
            return new DataResponse<EventResponse>
            {
                IsOk = false,
                ErrorMessage = "Event not found.",
            };

        return new DataResponse<EventResponse> { IsOk = true, Data = ev };
    }

    public async Task<BaseResponse> CreateEventAsync(
        CreateEventRequest request,
        Guid organizerId,
        string createdBy
    )
    {
        var ev = new Model.Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = EventStatusEnum.Draft,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        var rows = await _eventRepository.CreateAsync(ev);
        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Event created successfully." : "Failed to create event.",
            AnyChange = rows,
        };
    }

    public async Task<BaseResponse> UpdateEventAsync(
        UpdateEventRequest request,
        Guid organizerId,
        string updatedBy
    )
    {
        var existing = await _eventRepository.GetByIdAsync(request.Id);
        if (existing is null)
            return new BaseResponse { IsOk = false, ErrorMessage = "Event not found." };

        var ev = new Model.Event
        {
            Id = request.Id,
            OrganizerId = organizerId,
            Title = request.Title,
            Description = request.Description,
            Location = request.Location,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status,
            UpdatedOn = DateTime.UtcNow,
            UpdatedBy = updatedBy,
        };

        var rows = await _eventRepository.UpdateAsync(ev);
        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Event updated successfully." : "Failed to update event.",
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
}
