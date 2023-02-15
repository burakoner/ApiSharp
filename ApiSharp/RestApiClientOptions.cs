namespace ApiSharp;

public class RestApiClientOptions : BaseClientOptions
{
    /// <summary>
    /// Http client to use. If a HttpClient is provided in this property the RequestTimeout and Proxy options provided in these options will be ignored in requests and should be set on the provided HttpClient instance
    /// </summary>
    public HttpClient HttpClient { get; set; }
    public HttpOptions HttpOptions { get; set; }

    // Rate Limiters
    public bool IgnoreRateLimiters { get; set; }
    public List<IRateLimiter> RateLimiters { get; set; }
    public RateLimitingBehavior RateLimitingBehavior { get; set; }

    // Request Body
    public string RequestBodyParameterKey { get; set; } = "";
    public string RequestBodyEmptyContent { get; set; } = "";
    public IEnumerable<HttpMethod> SetRequestBodyEmptyContentMethods { get; set; } = new List<HttpMethod>();

    public RestApiClientOptions() : this(string.Empty) { }
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
            BodyFormat = RestBodyFormat.Json,
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
