namespace ApiSharp;

/// <summary>
/// Base Client Options
/// </summary>
public abstract class BaseClientOptions
{
    /// <summary>
    /// Base Address
    /// </summary>
    public string BaseAddress { get; set; }

    /// <summary>
    /// Encoding
    /// </summary>
    public Encoding Encoding { get; set; }

    /// <summary>
    /// Json Options
    /// </summary>
    public JsonOptions JsonOptions { get; set; }

    /// <summary>
    /// Debug Mode
    /// </summary>
    public bool DebugMode { get; set; }

    /// <summary>
    /// Output
    /// </summary>
    public bool RawResponse { get; set; }

    /// <summary>
    /// Proxy
    /// </summary>
    public ProxyCredentials Proxy { get; set; }

    /// <summary>
    /// ApiCredentials
    /// </summary>
    public ApiCredentials ApiCredentials { get; set; }

    /// <summary>
    /// Authentication Provider
    /// </summary>
    public AuthenticationProvider AuthenticationProvider { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public BaseClientOptions()
    {
        // Encoding
        Encoding = Encoding.UTF8;

        // Json Options
        JsonOptions = new JsonOptions
        {
            ErrorBehavior = ErrorBehavior.ThrowException,
        };

        // Output
        RawResponse = false;

        // Authentication
        ApiCredentials = null;
        AuthenticationProvider = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="clientOptions"></param>
    public BaseClientOptions(BaseClientOptions clientOptions)
    {
        // Check Point
        if (clientOptions == null)
            return;

        // Encoding
        Encoding = clientOptions.Encoding ?? Encoding.UTF8;

        // Json Options
        JsonOptions = clientOptions.JsonOptions ?? new JsonOptions
        {
            ErrorBehavior = ErrorBehavior.ThrowException,
        };

        // Output
        RawResponse = clientOptions.RawResponse;

        // Proxy
        Proxy = clientOptions.Proxy;

        // Authentication
        ApiCredentials = clientOptions.ApiCredentials?.Copy();
        // AuthenticationProvider = new AuthenticationProvider(ApiCredentials);
    }

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"BaseAddress: {BaseAddress}, Encoding: {Encoding}, RawResponse: {RawResponse}, Proxy: {(Proxy == null ? "-" : Proxy.Host)}";
    }
}
