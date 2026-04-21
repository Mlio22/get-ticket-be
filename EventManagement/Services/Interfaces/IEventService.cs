using Common.DTO;
using EventManagement.DTO.Event;

namespace EventManagement.Services.Interfaces;

public interface IEventService
{
    Task<ListResponse<EventResponse>> GetAllEventsAsync(bool includeUnpublished = false);
    Task<DataResponse<EventResponse>> GetEventByIdAsync(Guid id);
    Task<BaseResponse> CreateEventAsync(
        CreateEventRequest request,
        Guid organizerId,
        string createdBy
    );
    Task<BaseResponse> UpdateEventAsync(
        UpdateEventRequest request,
        Guid organizerId,
        string updatedBy
    );
    Task<BaseResponse> DeleteEventAsync(Guid id, Guid organizerId, string updatedBy);
}
