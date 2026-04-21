namespace Common.Utils;

/// <summary>Timezone helper utilities. Default app timezone is Asia/Jakarta (WIB, UTC+7).</summary>
public static class Timezone
{
    private static readonly TimeZoneInfo _jakarta = TimeZoneInfo.FindSystemTimeZoneById(
        "Asia/Jakarta"
    );

    /// <summary>Convert a UTC DateTime to Jakarta time (WIB).</summary>
    public static DateTime ToJakarta(DateTime utcTime)
    {
        var dt =
            utcTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(utcTime, DateTimeKind.Utc)
                : utcTime;
        return TimeZoneInfo.ConvertTimeFromUtc(dt, _jakarta);
    }

    /// <summary>Convert a Jakarta DateTime back to UTC.</summary>
    public static DateTime ToUtc(DateTime jakartaTime)
    {
        var dt =
            jakartaTime.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(jakartaTime, DateTimeKind.Unspecified)
                : jakartaTime;
        return TimeZoneInfo.ConvertTimeToUtc(dt, _jakarta);
    }

    /// <summary>Current time in UTC.</summary>
    public static DateTime UtcNow() => DateTime.UtcNow;

    /// <summary>Current time in Jakarta (WIB).</summary>
    public static DateTime NowJakarta() => ToJakarta(DateTime.UtcNow);

    /// <summary>Today's date in Jakarta.</summary>
    public static DateOnly TodayJakarta() => DateOnly.FromDateTime(NowJakarta());
}
