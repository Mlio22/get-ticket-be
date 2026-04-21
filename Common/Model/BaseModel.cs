using System.Text.Json.Serialization;
using Common.Model.Interfaces;
using Dapper.Contrib.Extensions;

namespace Common.Model;

public abstract class BaseModel<TId> : IAuditable, IIdentifiable<TId>
{
    [ExplicitKey]
    public TId Id { get; set; } = default!;

    [JsonIgnore]
    public DateTime CreatedOn { get; set; }

    [JsonIgnore]
    public string CreatedBy { get; set; } = string.Empty;

    [JsonIgnore]
    public DateTime? UpdatedOn { get; set; }

    [JsonIgnore]
    public string? UpdatedBy { get; set; }
}
