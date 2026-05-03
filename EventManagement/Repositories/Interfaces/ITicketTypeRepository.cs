using Common.DTO;
using EventManagement.DTO.Event;

namespace EventManagement.Repositories.Interfaces;

public interface ITicketTypeRepository
{
    Task<ListResponse<TicketTypeResponse>> GetByEventIdAsync(Guid eventId);
    Task<IReadOnlyDictionary<Guid, List<TicketTypeResponse>>> GetByEventIdsAsync(
        IEnumerable<Guid> eventIds
    );
    Task<TicketTypeResponse?> GetByIdAsync(Guid id);
    Task<int> CreateAsync(Model.TicketType ticketType);
    Task<int> UpdateAsync(Model.TicketType ticketType);
    Task<int> SoftDeleteAsync(Guid id, string updatedBy);
}
