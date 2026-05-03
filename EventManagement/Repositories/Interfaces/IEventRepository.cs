using Common.DTO;
using EventManagement.DTO.Event;

namespace EventManagement.Repositories.Interfaces;

public interface IEventRepository
{
    Task<ListResponse<EventResponse>> GetAllAsync(bool includeUnpublished = false);
    Task<ListResponse<EventResponse>> GetByOrganizerIdAsync(Guid organizerId);
    Task<EventResponse?> GetByIdAsync(Guid id);
    Task<int> CreateAsync(Model.Event ev);
    Task<int> UpdateAsync(Model.Event ev);
    Task<int> SoftDeleteAsync(Guid id, string updatedBy);
}
