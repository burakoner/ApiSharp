namespace ApiSharp.Throttling.Abstractions;

public abstract class Limiter
{
    public HttpMethod Method { get; set; }
    public RateLimiterType Type { get; set; }

    public int Limit { get; set; }
    public TimeSpan Period { get; set; }
    public SemaphoreSlim Semaphore { get; set; }

    public bool IgnoreOtherRateLimits { get; set; }

    internal List<LimitEntry> Entries { get; set; } = new List<LimitEntry>();

    public Limiter(RateLimiterType type, int limit, TimeSpan period, HttpMethod method, bool ignoreOtherRateLimits)
    {
        Type = type;
        Method = method;

        Limit = limit;
        Period = period;
        Semaphore = new SemaphoreSlim(1, 1);

        IgnoreOtherRateLimits = ignoreOtherRateLimits;
    }
}
