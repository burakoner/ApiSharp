namespace ApiSharp.Throttling.Interfaces;

/// <summary>
/// Rate limiter interface
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Limit a request based on previous requests made
    /// </summary>
    /// <param name="log">The logger</param>
    /// <param name="endpoint">The endpoint the request is for</param>
    /// <param name="method">The Http request method</param>
    /// <param name="signed">Whether the request is singed(private) or not</param>
    /// <param name="apikey">The api key making this request</param>
    /// <param name="limitBehaviour">The limit behavior for when the limit is reached</param>
    /// <param name="requestWeight">The weight of the request</param>
    /// <param name="ct">Cancellation token to cancel waiting</param>
    /// <returns>The time in milliseconds spend waiting</returns>
    Task<CallResult<int>> LimitRequestAsync(ILogger logger, string endpoint, HttpMethod method, bool signed, SensitiveString apikey, RateLimitingBehavior limitBehaviour, int requestWeight, CancellationToken ct);
}
