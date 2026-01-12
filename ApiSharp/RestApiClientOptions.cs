namespace ApiSharp;

/// <summary>
/// RestApiClientOptions
/// </summary>
public class RestApiClientOptions : BaseClientOptions
{
    /// <summary>
    /// Http client to use. If a HttpClient is provided in this property the RequestTimeout and Proxy options provided in these options will be ignored in requests and should be set on the provided HttpClient instance
    /// </summary>
    public HttpClient? HttpClient { get; set; }

    /// <summary>
    /// Http Options
    /// </summary>
    public HttpOptions HttpOptions { get; set; }

    /// <summary>
    /// Ignore Rate Limiters
    /// </summary>
    [Obsolete]
    public bool IgnoreRateLimiters { get; set; }

    /// <summary>
    /// Rate Limiters
    /// </summary>
    [Obsolete]
    public List<IRateLimiter> RateLimiters { get; set; } = [];

    /// <summary>
    /// Whether or not client side rate limiting should be applied
    /// </summary>
    public bool RateLimiterEnabled { get; set; } = true;

    /// <summary>
    /// What should happen when a rate limit is reached
    /// </summary>
    public RateLimitingBehavior RateLimitingBehavior { get; set; } = RateLimitingBehavior.Wait;

    /// <summary>
    /// Gets or sets the collection of HTTP methods for which the request body should be set to empty content.
    /// </summary>
    public IEnumerable<HttpMethod> SetRequestBodyEmptyContentMethods { get; set; } = [];

    /// <summary>
    /// Constructor
    /// </summary>
    public RestApiClientOptions() : this(string.Empty) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseAddress"></param>
    public RestApiClientOptions(string baseAddress)
    {
        // Base Address
        this.BaseAddress = baseAddress;

        // Encoding
        this.Encoding = Encoding.UTF8;

        // Http Options
        this.HttpOptions = new HttpOptions
        {
            UserAgent = RestApiConstants.USER_AGENT,
            AcceptMimeType = RestApiConstants.JSON_CONTENT_HEADER,
            RequestTimeout = TimeSpan.FromSeconds(30),
            EncodeQueryString = true,
        };

        // Json Options
        this.JsonOptions = new JsonOptions
        {
            ErrorBehavior = ErrorBehavior.ThrowException,
        };

        // Output
        this.RawResponse = false;

        // Proxy
        this.Proxy = null;

        // Rate Limiters
        this.IgnoreRateLimiters = false;
        this.RateLimiters = new List<IRateLimiter>();
        this.RateLimitingBehavior = RateLimitingBehavior.Wait;
    }
}
