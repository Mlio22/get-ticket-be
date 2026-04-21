using Common.Model;
using Dapper.Contrib.Extensions;
using EventManagement.Enums;

namespace EventManagement.Model;

[Table("events")]
public class Event : BaseModel<Guid>
{
    public Guid OrganizerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatusEnum Status { get; set; } = EventStatusEnum.Draft;
    public bool IsDeleted { get; set; } = false;
}
