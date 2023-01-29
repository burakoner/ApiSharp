namespace ApiSharp.Throttling.Limiters;

internal class PartialEndpointRateLimiter : Limiter
{
    public string[] PartialEndpoints { get; set; }
    public bool CountPerEndpoint { get; set; }

    public PartialEndpointRateLimiter(string[] partialEndpoints, int limit, TimeSpan period, bool countPerEndpoint, HttpMethod method, bool ignoreOtherRateLimits)
        : base(RateLimiterType.PartialEndpoint, limit, period, method, ignoreOtherRateLimits)
    {
        PartialEndpoints = partialEndpoints;
        CountPerEndpoint = countPerEndpoint;
    }
}
