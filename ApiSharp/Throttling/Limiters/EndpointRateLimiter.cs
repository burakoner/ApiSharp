namespace ApiSharp.Throttling;

internal class EndpointRateLimiter(string[] endpoints, int limit, TimeSpan period, HttpMethod? method, bool ignoreOtherRateLimits) : Limiter(RateLimiterType.Endpoint, limit, period, method, ignoreOtherRateLimits)
{
    public string[] Endpoints { get; set; } = endpoints;
}
