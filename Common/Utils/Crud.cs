using System.Data;
using Common.DB;
using Dapper;

namespace Common.Utils;

/// <summary>
/// Generic CRUD helpers wrapping Dapper. Use these instead of writing
/// raw queries inline in repositories for common operations.
/// </summary>
public static class Crud
{
    /// <summary>Query a list of T using a raw SQL string.</summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null
    )
    {
        return await db.QueryAsync<T>(sql, param, transaction);
    }

    /// <summary>Query a single T or default (null) if not found.</summary>
    public static async Task<T?> QueryFirstOrDefaultAsync<T>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null
    )
    {
        return await db.QueryFirstOrDefaultAsync<T>(sql, param, transaction);
    }

    /// <summary>Execute an INSERT and return rows affected.</summary>
    public static async Task<int> InsertAsync(
        IDbConnection db,
        string sql,
        object param,
        IDbTransaction? transaction = null
    )
    {
        return await db.ExecuteAsync(sql, param, transaction);
    }

    /// <summary>Execute an UPDATE and return rows affected.</summary>
    public static async Task<int> UpdateAsync(
        IDbConnection db,
        string sql,
        object param,
        IDbTransaction? transaction = null
    )
    {
        return await db.ExecuteAsync(sql, param, transaction);
    }

    /// <summary>Execute a DELETE (or soft-delete) and return rows affected.</summary>
    public static async Task<int> DeleteAsync(
        IDbConnection db,
        string sql,
        object param,
        IDbTransaction? transaction = null
    )
    {
        return await db.ExecuteAsync(sql, param, transaction);
    }

    /// <summary>Execute any SQL statement and return rows affected.</summary>
    public static async Task<int> ExecuteAsync(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null
    )
    {
        return await db.ExecuteAsync(sql, param, transaction);
    }

    // ── Context shortcuts ─────────────────────────────────────────

    /// <summary>Query a list using an IDbContext (auto-closes).</summary>
    public static async Task<IEnumerable<T>> QueryAsync<T>(
        IDbContext ctx,
        string sql,
        object? param = null
    ) => await QueryAsync<T>(ctx.DB, sql, param, ctx.Transaction);

    /// <summary>Query a single T or default using an IDbContext.</summary>
    public static async Task<T?> QueryFirstOrDefaultAsync<T>(
        IDbContext ctx,
        string sql,
        object? param = null
    ) => await QueryFirstOrDefaultAsync<T>(ctx.DB, sql, param, ctx.Transaction);

    /// <summary>Execute INSERT using an IDbContext.</summary>
    public static async Task<int> InsertAsync(IDbContext ctx, string sql, object param) =>
        await InsertAsync(ctx.DB, sql, param, ctx.Transaction);

    /// <summary>Execute UPDATE using an IDbContext.</summary>
    public static async Task<int> UpdateAsync(IDbContext ctx, string sql, object param) =>
        await UpdateAsync(ctx.DB, sql, param, ctx.Transaction);

    /// <summary>Execute DELETE using an IDbContext.</summary>
    public static async Task<int> DeleteAsync(IDbContext ctx, string sql, object param) =>
        await DeleteAsync(ctx.DB, sql, param, ctx.Transaction);
}
