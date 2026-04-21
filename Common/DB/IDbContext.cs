using System.Data;

namespace Common.DB;

public interface IDbContext : IDisposable
{
    IDbConnection DB { get; }
    IDbTransaction? Transaction { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
