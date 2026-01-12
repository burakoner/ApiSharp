namespace ApiSharp.Throttling;

internal class TotalRateLimiter(int limit, TimeSpan period, HttpMethod? method, bool ignoreOtherRateLimits) : Limiter(RateLimiterType.Total, limit, period, method, ignoreOtherRateLimits)
{
}
