namespace ApiSharp.Throttling.Limiters;

internal class ApiKeyRateLimiter : Limiter
{
    public bool OnlyForSignedRequests { get; set; }

    public ApiKeyRateLimiter(int limit, TimeSpan period, bool onlyForSignedRequests, HttpMethod method, bool ignoreOtherRateLimits)
        : base(RateLimiterType.ApiKey, limit, period, method, ignoreOtherRateLimits)
    {
        OnlyForSignedRequests = onlyForSignedRequests;
    }
}