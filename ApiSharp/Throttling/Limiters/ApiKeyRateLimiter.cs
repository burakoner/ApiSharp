namespace ApiSharp.Throttling;

internal class ApiKeyRateLimiter(int limit, TimeSpan period, bool onlyForSignedRequests, HttpMethod? method, bool ignoreOtherRateLimits) : Limiter(RateLimiterType.ApiKey, limit, period, method, ignoreOtherRateLimits)
{
    public bool OnlyForSignedRequests { get; set; } = onlyForSignedRequests;
}