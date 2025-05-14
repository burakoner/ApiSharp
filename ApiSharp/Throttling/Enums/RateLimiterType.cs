namespace ApiSharp.Throttling.Enums;

/// <summary>
/// Rate Limiter Type
/// </summary>
public enum RateLimiterType:byte
{
    /// <summary>
    /// Total
    /// </summary>
    Total=1,

    /// <summary>
    /// Endpoint
    /// </summary>
    Endpoint = 2,

    /// <summary>
    /// Partial Endpoint
    /// </summary>
    PartialEndpoint = 3,

    /// <summary>
    /// Api Key
    /// </summary>
    ApiKey = 4
}
