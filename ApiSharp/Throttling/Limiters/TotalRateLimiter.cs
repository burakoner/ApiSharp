namespace ApiSharp.Throttling.Limiters;

internal class TotalRateLimiter(int limit, TimeSpan period, HttpMethod? method, bool ignoreOtherRateLimits) : Limiter(RateLimiterType.Total, limit, period, method, ignoreOtherRateLimits)
{
}
