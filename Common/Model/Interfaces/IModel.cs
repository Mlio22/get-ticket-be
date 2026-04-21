namespace Common.Model.Interfaces;

public interface IIdentifiable<T>
{
    T Id { get; set; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

public interface IActivatable
{
    bool IsActive { get; set; }
}
