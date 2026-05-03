using EventManagement.DTO.Event;

namespace EventManagement.Repositories.Interfaces;

public interface IUserLookupRepository
{
    Task<OrganizerResponse?> GetByIdAsync(Guid id);
    Task<IReadOnlyDictionary<Guid, OrganizerResponse>> GetByIdsAsync(IEnumerable<Guid> ids);
}
