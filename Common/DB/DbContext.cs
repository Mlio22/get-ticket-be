using System.Data;
using Npgsql;

namespace Common.DB;

public class DbContext : IDbContext
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;

    public DbContext(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public IDbConnection DB => _connection;
    public IDbTransaction? Transaction => _transaction;

    public Task BeginTransactionAsync()
    {
        _transaction = _connection.BeginTransaction();
        return Task.CompletedTask;
    }

    public Task CommitAsync()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public Task RollbackAsync()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _connection.Dispose();
    }
}
