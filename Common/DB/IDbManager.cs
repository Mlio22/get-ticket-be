using System.Data;

namespace Common.DB;

public interface IDbManager
{
    string ConnectionString { get; }
    IDbConnection CreateConnection();
    IDbContext CreateContext();
}
