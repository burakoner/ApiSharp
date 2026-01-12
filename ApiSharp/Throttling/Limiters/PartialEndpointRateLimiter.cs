namespace ApiSharp.Throttling;

internal class PartialEndpointRateLimiter(string[] partialEndpoints, int limit, TimeSpan period, bool countPerEndpoint, HttpMethod? method, bool ignoreOtherRateLimits) : Limiter(RateLimiterType.PartialEndpoint, limit, period, method, ignoreOtherRateLimits)
{
    public string[] PartialEndpoints { get; set; } = partialEndpoints;
    public bool CountPerEndpoint { get; set; } = countPerEndpoint;
}
