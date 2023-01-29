namespace ApiSharp.Extensions;

public static class DateTimeExtensions
{
    private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const long _ticksPerSecond = TimeSpan.TicksPerMillisecond * 1000;
    private const decimal _ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000m;
    private const decimal _ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000m / 1000m;

    public static DateTime ConvertFromSeconds(this int seconds) => _epoch.AddTicks(seconds * _ticksPerSecond);
    public static DateTime ConvertFromSeconds(this long seconds) => _epoch.AddTicks(seconds * _ticksPerSecond);
    public static DateTime ConvertFromSeconds(this double seconds) => _epoch.AddTicks((long)Math.Round(seconds * _ticksPerSecond));

    public static DateTime ConvertFromMilliseconds(this int milliseconds) => _epoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);
    public static DateTime ConvertFromMilliseconds(this long milliseconds) => _epoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);
    public static DateTime ConvertFromMilliseconds(this double milliseconds) => _epoch.AddTicks((long)Math.Round(milliseconds * TimeSpan.TicksPerMillisecond));

    public static DateTime ConvertFromMicroseconds(this long microseconds) => _epoch.AddTicks((long)Math.Round(microseconds * _ticksPerMicrosecond));
    public static DateTime ConvertFromNanoseconds(this long nanoseconds) => _epoch.AddTicks((long)Math.Round(nanoseconds * _ticksPerNanosecond));
    
    public static long ConvertToSeconds(this DateTime time) => (long)Math.Round((time - _epoch).TotalSeconds);
    public static long? ConvertToSeconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalSeconds);
    
    public static long ConvertToMilliseconds(this DateTime time) => (long)Math.Round((time - _epoch).TotalMilliseconds);
    public static long? ConvertToMilliseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalMilliseconds);
    
    public static long ConvertToMicroseconds(this DateTime time) =>  (long)Math.Round((time - _epoch).Ticks / _ticksPerMicrosecond);
    public static long? ConvertToMicroseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerMicrosecond);
    
    public static long ConvertToNanoseconds(this DateTime time) => (long)Math.Round((time - _epoch).Ticks / _ticksPerNanosecond);
    public static long? ConvertToNanoseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerNanosecond);
}
