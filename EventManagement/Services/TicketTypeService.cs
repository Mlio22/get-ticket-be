using Common.DTO;
using EventManagement.DTO.Event;
using EventManagement.Enums;
using EventManagement.Repositories.Interfaces;
using EventManagement.Services.Interfaces;

namespace EventManagement.Services;

public class TicketTypeService : ITicketTypeService
{
    private readonly ITicketTypeRepository _ticketTypeRepository;

    public TicketTypeService(ITicketTypeRepository ticketTypeRepository)
    {
        _ticketTypeRepository = ticketTypeRepository;
    }

    public Task<ListResponse<TicketTypeResponse>> GetByEventIdAsync(Guid eventId) =>
        _ticketTypeRepository.GetByEventIdAsync(eventId);

    public async Task<DataResponse<TicketTypeResponse>> GetByIdAsync(Guid id)
    {
        var tt = await _ticketTypeRepository.GetByIdAsync(id);
        if (tt is null)
            return new DataResponse<TicketTypeResponse>
            {
                IsOk = false,
                ErrorMessage = "Ticket type not found.",
            };

        return new DataResponse<TicketTypeResponse> { IsOk = true, Data = tt };
    }

    public async Task<BaseResponse> CreateAsync(CreateTicketTypeRequest request, string createdBy)
    {
        var ticketType = new Model.TicketType
        {
            Id = Guid.NewGuid(),
            EventId = request.EventId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            TotalSeats = request.TotalSeats,
            AvailableSeats = request.TotalSeats,
            Status = TicketStatusEnum.Available,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = createdBy,
        };

        var rows = await _ticketTypeRepository.CreateAsync(ticketType);
        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Ticket type created." : "Failed to create ticket type.",
            AnyChange = rows,
        };
    }

    public async Task<BaseResponse> DeleteAsync(Guid id, string updatedBy)
    {
        var existing = await _ticketTypeRepository.GetByIdAsync(id);
        if (existing is null)
            return new BaseResponse { IsOk = false, ErrorMessage = "Ticket type not found." };

        var rows = await _ticketTypeRepository.SoftDeleteAsync(id, updatedBy);
        return new BaseResponse
        {
            IsOk = rows > 0,
            Message = rows > 0 ? "Ticket type deleted." : "Failed to delete ticket type.",
            AnyChange = rows,
        };
    }
}
