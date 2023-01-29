namespace ApiSharp.Throttling.Enums;

/// <summary>
/// What to do when a request would exceed the rate limit
/// </summary>
public enum RateLimitingBehavior
{
    /// <summary>
    /// Fail the request
    /// </summary>
    Fail,

    /// <summary>
    /// Wait till the request can be send
    /// </summary>
    Wait
}
