using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Common.DB;

public class DbManager : IDbManager
{
    public string ConnectionString { get; }

    public DbManager(IConfiguration configuration)
    {
        ConnectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured."
            );
    }

    public IDbConnection CreateConnection()
    {
        var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    public IDbContext CreateContext() => new DbContext(ConnectionString);
}
