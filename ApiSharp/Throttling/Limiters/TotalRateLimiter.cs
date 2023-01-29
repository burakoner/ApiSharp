namespace ApiSharp.Throttling.Limiters;

internal class TotalRateLimiter : Limiter
{
    public TotalRateLimiter(int limit, TimeSpan period, HttpMethod method, bool ignoreOtherRateLimits)
        : base(RateLimiterType.Total, limit, period, method, ignoreOtherRateLimits)
    {
    }
}
