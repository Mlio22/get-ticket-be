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
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public string PosterImage { get; set; } = string.Empty;
    public string BannerImage { get; set; } = string.Empty;
    public bool IsFeatured { get; set; } = false;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public EventStatusEnum Status { get; set; } = EventStatusEnum.Draft;
    public bool IsDeleted { get; set; } = false;
}
