using Common.DTO;
using EventManagement.DTO.Event;

namespace EventManagement.Services.Interfaces;

public interface IEventService
{
    Task<ListResponse<EventResponse>> GetAllEventsAsync(bool includeUnpublished = false);
    Task<ListResponse<EventResponse>> GetMyEventsAsync(Guid organizerId);
    Task<DataResponse<OrganizerDashboardResponse>> GetOrganizerDashboardAsync(Guid organizerId);
    Task<DataResponse<EventResponse>> GetEventByIdAsync(Guid id);
    Task<BaseResponse> CreateEventAsync(
        UpsertEventRequest request,
        Guid organizerId,
        string createdBy
    );
    Task<BaseResponse> UpdateEventAsync(
        Guid eventId,
        UpsertEventRequest request,
        Guid organizerId,
        string updatedBy
    );
    Task<BaseResponse> DeleteEventAsync(Guid id, Guid organizerId, string updatedBy);
}
