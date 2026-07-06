namespace BuildingBlocks.CrossCutting.ExtensionMethods;

public static class DateOnlyExtensions
{
    /// <summary>
    /// Nếu sau có xử lý liên quan đến thời gian thì thay thành DateTimeOffset
    /// </summary>
    public static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    public static DateTimeOffset ToStartOfDayUtc(this DateOnly date)
    => new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, VnOffset).ToUniversalTime();
    public static DateTimeOffset? ToStartOfDayUtc(this DateOnly? date)
    => date.HasValue ? new DateTimeOffset(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0, VnOffset).ToUniversalTime() : null;

    /// <summary>
    /// Converts DateOnly to DateTimeOffset at current time of day with Vietnamese timezone offset, then converts to UTC
    /// </summary>
    public static DateTimeOffset ToCurrentTimeOfDayUtc(this DateOnly date)
    {
        var now = DateTime.Now;
        return new DateTimeOffset(date.Year, date.Month, date.Day, now.Hour, now.Minute, now.Second, VnOffset).ToUniversalTime();
    }

    /// <summary>
    /// Converts nullable DateOnly to DateTimeOffset at current time of day with Vietnamese timezone offset, then converts to UTC
    /// </summary>
    public static DateTimeOffset? ToCurrentTimeOfDayUtc(this DateOnly? date)
    => date.HasValue ? date.Value.ToCurrentTimeOfDayUtc() : null;

    public static DateTimeOffset? ToEndOfDayUtc(this DateOnly? date)
    => date.HasValue ? new DateTimeOffset(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59, VnOffset).ToUniversalTime() : null;
    public static DateTimeOffset ToEndOfDayUtc(this DateOnly date)
    => new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, VnOffset).ToUniversalTime();
    /// <summary>
    /// Converts DateOnly to DateTime at the start of the day (00:00:00)
    /// </summary>
    public static DateTime ToStartOfDay(this DateOnly date)
    => date.ToDateTime(TimeOnly.MinValue);

    /// <summary>
    /// Converts DateOnly to DateTime at the end of the day (23:59:59.9999999)
    /// </summary>
    public static DateTime ToEndOfDay(this DateOnly date)
    => date.ToDateTime(TimeOnly.MaxValue);

    /// <summary>
    /// Converts DateTimeOffset to DateOnly with Vietnamese timezone offset
    /// </summary>
    public static DateOnly ToDateOnlyVn(this DateTimeOffset date)
    => DateOnly.FromDateTime(date.DateTime.Add(VnOffset));
    /// <summary>
    /// Converts DateTimeOffset to DateOnly with Vietnamese timezone offset
    /// </summary>
    public static DateOnly? ToDateOnly(this DateTime? date)
    => date.HasValue ? DateOnly.FromDateTime(date.Value) : null;
    /// <summary>
    /// Converts nullable DateTimeOffset to DateOnly with Vietnamese timezone offset
    /// </summary>
    public static DateOnly? ToDateOnlyVn(this DateTimeOffset? date)
    => date.HasValue ? DateOnly.FromDateTime(date.Value.DateTime.Add(VnOffset)) : null;

}