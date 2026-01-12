namespace ApiSharp.Throttling;

/// <summary>
/// Limiter Abstract Class
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="type">Type</param>
/// <param name="limit">Limit</param>
/// <param name="period">Period</param>
/// <param name="method">Method</param>
/// <param name="ignoreOtherRateLimits">Ignore Other Rate Limits</param>
public abstract class Limiter(RateLimiterType type, int limit, TimeSpan period, HttpMethod? method, bool ignoreOtherRateLimits)
{
    /// <summary>
    /// Method
    /// </summary>
    public HttpMethod? Method { get; set; } = method;

    /// <summary>
    /// Type of Rate Limiter
    /// </summary>
    public RateLimiterType Type { get; set; } = type;

    /// <summary>
    /// Limit
    /// </summary>
    public int Limit { get; set; } = limit;

    /// <summary>
    /// Period
    /// </summary>
    public TimeSpan Period { get; set; } = period;

    /// <summary>
    /// SemaphoreSlim
    /// </summary>
    public SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Ignore Other Rate Limits
    /// </summary>
    public bool IgnoreOtherRateLimits { get; set; } = ignoreOtherRateLimits;

    internal List<LimitEntry> Entries { get; set; } = [];
}
