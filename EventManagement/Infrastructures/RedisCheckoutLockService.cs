using Common.Exceptions;
using StackExchange.Redis;

namespace EventManagement.Infrastructures;

public class RedisCheckoutLockService : ICheckoutLockService
{
    private const string ExpirationIndexKey = "checkout:expirations";

    private readonly IConnectionMultiplexer _redis;

    public RedisCheckoutLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task EnsureTicketStockAsync(
        Guid ticketTypeId,
        int dbAvailableSeats,
        int activeReservedQuantity
    )
    {
        var db = _redis.GetDatabase();
        var stock = Math.Max(dbAvailableSeats - activeReservedQuantity, 0);
        await db.StringSetAsync(GetStockKey(ticketTypeId), stock, when: When.NotExists);
    }

    public async Task<CheckoutReservationResult> ReserveAsync(
        Guid checkoutId,
        Guid ticketTypeId,
        int quantity,
        DateTime expiresAt
    )
    {
        var db = _redis.GetDatabase();
        var expiresAtUnixMs = new DateTimeOffset(expiresAt).ToUnixTimeMilliseconds();
        var rawResult = await db.ScriptEvaluateAsync(
            """
            local stockKey = KEYS[1]
            local holdKey = KEYS[2]
            local expirationKey = KEYS[3]

            if redis.call('EXISTS', holdKey) == 1 then
                local currentStock = tonumber(redis.call('GET', stockKey) or '-1')
                return {1, currentStock}
            end

            local stock = tonumber(redis.call('GET', stockKey) or '-1')
            local qty = tonumber(ARGV[1])

            if stock < 0 then
                return {-1, 0}
            end

            if stock < qty then
                return {0, stock}
            end

            redis.call('DECRBY', stockKey, qty)
            redis.call('HSET', holdKey, 'ticket_type_id', ARGV[2], 'quantity', qty)
            redis.call('PEXPIREAT', holdKey, ARGV[3])
            redis.call('ZADD', expirationKey, ARGV[3], ARGV[4])

            return {1, stock - qty}
            """,
            [GetStockKey(ticketTypeId), GetHoldKey(checkoutId, ticketTypeId), ExpirationIndexKey],
            [
                quantity,
                ticketTypeId.ToString("N"),
                expiresAtUnixMs,
                GetExpirationMember(checkoutId, ticketTypeId),
            ]
        );

        if (rawResult.IsNull)
            throw new InternalServerException(
                "Redis reservation script returned an invalid result."
            );

        RedisResult[]? result;
        try
        {
            result = (RedisResult[]?)rawResult;
        }
        catch (InvalidCastException)
        {
            throw new InternalServerException(
                "Redis reservation script returned an invalid result."
            );
        }

        if (result is not { Length: >= 2 })
            throw new InternalServerException(
                "Redis reservation script returned an invalid result."
            );

        return new CheckoutReservationResult
        {
            IsReserved = (int)result[0] == 1,
            RemainingStock = (int)result[1],
        };
    }

    public async Task FinalizeReservationAsync(Guid checkoutId, Guid ticketTypeId)
    {
        var db = _redis.GetDatabase();
        await db.ScriptEvaluateAsync(
            """
            local holdKey = KEYS[1]
            local expirationKey = KEYS[2]
            redis.call('DEL', holdKey)
            redis.call('ZREM', expirationKey, ARGV[1])
            return 1
            """,
            [GetHoldKey(checkoutId, ticketTypeId), ExpirationIndexKey],
            [GetExpirationMember(checkoutId, ticketTypeId)]
        );
    }

    public async Task ReleaseReservationAsync(Guid checkoutId, Guid ticketTypeId, int quantity)
    {
        var db = _redis.GetDatabase();
        await db.ScriptEvaluateAsync(
            """
            local stockKey = KEYS[1]
            local holdKey = KEYS[2]
            local expirationKey = KEYS[3]
            local qty = tonumber(ARGV[2])

            if redis.call('EXISTS', stockKey) == 1 then
                if redis.call('EXISTS', holdKey) == 1 then
                    qty = tonumber(redis.call('HGET', holdKey, 'quantity') or ARGV[2])
                end

                redis.call('INCRBY', stockKey, qty)
            end

            redis.call('DEL', holdKey)
            redis.call('ZREM', expirationKey, ARGV[1])
            return 1
            """,
            [GetStockKey(ticketTypeId), GetHoldKey(checkoutId, ticketTypeId), ExpirationIndexKey],
            [GetExpirationMember(checkoutId, ticketTypeId), quantity]
        );
    }

    private static string GetStockKey(Guid ticketTypeId) => $"checkout:stock:{ticketTypeId:N}";

    private static string GetHoldKey(Guid checkoutId, Guid ticketTypeId) =>
        $"checkout:hold:{checkoutId:N}:{ticketTypeId:N}";

    private static string GetExpirationMember(Guid checkoutId, Guid ticketTypeId) =>
        $"{checkoutId:N}:{ticketTypeId:N}";
}
