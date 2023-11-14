namespace ApiSharp;

public abstract class BaseClientOptions
{
    // Base Address
    public string BaseAddress { get; set; }

    // Encoding
    public Encoding Encoding { get; set; }

    // Json Options
    public JsonOptions JsonOptions { get; set; }

    // Debug Mode
    public bool DebugMode { get; set; }

    // Output
    public bool RawResponse { get; set; }

    // Proxy
    public ProxyCredentials Proxy { get; set; }

    // Authentication
    public ApiCredentials ApiCredentials { get; set; }
    public AuthenticationProvider AuthenticationProvider { get; set; }

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

    public override string ToString()
    {
        return $"BaseAddress: {BaseAddress}, Encoding: {Encoding}, RawResponse: {RawResponse}, Proxy: {(Proxy == null ? "-" : Proxy.Host)}";
    }
}
