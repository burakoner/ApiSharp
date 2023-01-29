namespace ApiSharp;

public abstract class BaseClientOptions
{
    // Base Address
    public string BaseAddress { get; set; }

    // Encoding
    public Encoding Encoding { get; set; }

    // Json Options
    public JsonOptions JsonOptions { get; set; }

    // Output
    public bool OutputOriginalData { get; set; }

    // Proxy
    public ProxyCredentials Proxy { get; set; }

    // Authentication
    public ApiCredentials ApiCredentials { get; set; }
    public AuthenticationProvider AuthenticationProvider { get; set; }

    // Logging
    internal event Action OnLoggingChanged;
    private LogLevel _logLevel = LogLevel.Information;
    public LogLevel LogLevel
    {
        get => _logLevel;
        set
        {
            _logLevel = value;
            OnLoggingChanged?.Invoke();
        }
    }
    private List<ILogger> _logWriters = new List<ILogger> { new DebugLogger() };
    public List<ILogger> LogWriters
    {
        get => _logWriters;
        set
        {
            _logWriters = value;
            OnLoggingChanged?.Invoke();
        }
    }

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
        OutputOriginalData = false;

        // Authentication
        ApiCredentials = null;
        AuthenticationProvider = null;

        // Logging
        LogLevel = LogLevel.Information;
        LogWriters = new List<ILogger> { new DebugLogger() };
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
        OutputOriginalData = clientOptions.OutputOriginalData;

        // Proxy
        Proxy = clientOptions.Proxy;

        // Logging
        LogLevel = clientOptions.LogLevel;
        LogWriters = clientOptions.LogWriters.ToList();

        // Authentication
        ApiCredentials = clientOptions.ApiCredentials?.Copy();
        // AuthenticationProvider = new AuthenticationProvider(ApiCredentials);
    }

    public override string ToString()
    {
        return $"LogLevel: {LogLevel}, Writers: {LogWriters.Count}, Proxy: {(Proxy == null ? "-" : Proxy.Host)}";
    }
}
