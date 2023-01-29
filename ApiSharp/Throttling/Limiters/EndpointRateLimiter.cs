namespace ApiSharp.Throttling.Limiters;

internal class EndpointRateLimiter : Limiter
{
    public string[] Endpoints { get; set; }

    public EndpointRateLimiter(string[] endpoints, int limit, TimeSpan period, HttpMethod method, bool ignoreOtherRateLimits)
        : base(RateLimiterType.Endpoint, limit, period, method, ignoreOtherRateLimits)
    {
        Endpoints = endpoints;
    }
}
