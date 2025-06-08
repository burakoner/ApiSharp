namespace ApiSharp.Extensions;

/// <summary>
/// DateTime extensions for converting to and from various time formats
/// </summary>
public static class DateTimeExtensions
{
    private static readonly DateTime _epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private const long _ticksPerSecond = TimeSpan.TicksPerMillisecond * 1000;
    private const decimal _ticksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000m;
    private const decimal _ticksPerNanosecond = TimeSpan.TicksPerMillisecond / 1000m / 1000m;

    /// <summary>
    /// Convert a number of seconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="seconds">Seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromSeconds(this int seconds) => _epoch.AddTicks(seconds * _ticksPerSecond);

    /// <summary>
    /// Convert a number of seconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="seconds">Seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromSeconds(this long seconds) => _epoch.AddTicks(seconds * _ticksPerSecond);

    /// <summary>
    /// Convert a number of seconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="seconds">Seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromSeconds(this double seconds) => _epoch.AddTicks((long)Math.Round(seconds * _ticksPerSecond));

    /// <summary>
    /// Convert a number of milliseconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="milliseconds">Milli seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromMilliseconds(this int milliseconds) => _epoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);

    /// <summary>
    /// Convert a number of milliseconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="milliseconds">Milli seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromMilliseconds(this long milliseconds) => _epoch.AddTicks(milliseconds * TimeSpan.TicksPerMillisecond);

    /// <summary>
    /// Convert a number of milliseconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="milliseconds">Milli seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromMilliseconds(this double milliseconds) => _epoch.AddTicks((long)Math.Round(milliseconds * TimeSpan.TicksPerMillisecond));

    /// <summary>
    /// Convert a number of microseconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="microseconds">Nano seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromMicroseconds(this long microseconds) => _epoch.AddTicks((long)Math.Round(microseconds * _ticksPerMicrosecond));

    /// <summary>
    /// Convert a number of nanoseconds since the epoch (1970-01-01T00:00:00Z) to a DateTime
    /// </summary>
    /// <param name="nanoseconds">Nano seconds</param>
    /// <returns></returns>
    public static DateTime ConvertFromNanoseconds(this long nanoseconds) => _epoch.AddTicks((long)Math.Round(nanoseconds * _ticksPerNanosecond));

    /// <summary>
    /// Convert a DateTime to seconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long ConvertToSeconds(this DateTime time) => (long)Math.Round((time - _epoch).TotalSeconds);

    /// <summary>
    /// Convert a DateTime? to seconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long? ConvertToSeconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalSeconds);

    /// <summary>
    /// Convert a DateTime to milliseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long ConvertToMilliseconds(this DateTime time) => (long)Math.Round((time - _epoch).TotalMilliseconds);

    /// <summary>
    /// Convert a DateTime? to milliseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long? ConvertToMilliseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).TotalMilliseconds);

    /// <summary>
    /// Convert a DateTime to microseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long ConvertToMicroseconds(this DateTime time) =>  (long)Math.Round((time - _epoch).Ticks / _ticksPerMicrosecond);

    /// <summary>
    /// Convert a DateTime? to microseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long? ConvertToMicroseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerMicrosecond);

    /// <summary>
    /// Convert a DateTime to nanoseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long ConvertToNanoseconds(this DateTime time) => (long)Math.Round((time - _epoch).Ticks / _ticksPerNanosecond);

    /// <summary>
    /// Convert a DateTime? to nanoseconds since the epoch (1970-01-01T00:00:00Z)
    /// </summary>
    /// <param name="time">Time</param>
    /// <returns></returns>
    public static long? ConvertToNanoseconds(this DateTime? time) => time == null ? null : (long)Math.Round((time.Value - _epoch).Ticks / _ticksPerNanosecond);
}
