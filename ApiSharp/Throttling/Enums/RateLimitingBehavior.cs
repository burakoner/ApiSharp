namespace ApiSharp.Throttling;

/// <summary>
/// What to do when a request would exceed the rate limit
/// </summary>
public enum RateLimitingBehavior : byte
{
    /// <summary>
    /// Fail the request
    /// </summary>
    Fail = 1,

    /// <summary>
    /// Wait till the request can be send
    /// </summary>
    Wait = 2
}
