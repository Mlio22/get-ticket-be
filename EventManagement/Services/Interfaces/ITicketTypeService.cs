using Common.DTO;
using EventManagement.DTO.Event;

namespace EventManagement.Services.Interfaces;

public interface ITicketTypeService
{
    Task<ListResponse<TicketTypeResponse>> GetByEventIdAsync(Guid eventId);
    Task<DataResponse<TicketTypeResponse>> GetByIdAsync(Guid id);
    Task<BaseResponse> CreateAsync(CreateTicketTypeRequest request, string createdBy);
    Task<BaseResponse> DeleteAsync(Guid id, string updatedBy);
}
