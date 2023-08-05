namespace ApiSharp;

public class WebSocketApiClientOptions : BaseClientOptions
{
    /// <summary>
    /// Whether or not the socket should automatically reconnect when losing connection
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Time to wait between reconnect attempts
    /// </summary>
    public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Max number of concurrent resubscription tasks per socket after reconnecting a socket
    /// </summary>
    public int MaxConcurrentResubscriptionsPerConnection { get; set; } = 5;

    /// <summary>
    /// The max time to wait for a response after sending a request on the socket before giving a timeout
    /// </summary>
    public TimeSpan ResponseTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The max time of not receiving any data after which the connection is assumed to be dropped. This can only be used for socket connections where a steady flow of data is expected,
    /// for example when the server sends intermittent ping requests
    /// </summary>
    public TimeSpan NoDataTimeout { get; set; }

    /// <summary>
    /// The amount of subscriptions that should be made on a single socket connection. Not all API's support multiple subscriptions on a single socket.
    /// Setting this to a higher number increases subscription speed because not every subscription needs to connect to the server, but having more subscriptions on a 
    /// single connection will also increase the amount of traffic on that single connection, potentially leading to issues.
    /// </summary>
    public int? SubscriptionsCombineTarget { get; set; }

    /// <summary>
    /// The max amount of connections to make to the server. Can be used for API's which only allow a certain number of connections. Changing this to a high value might cause issues.
    /// </summary>
    public int? MaxConnections { get; set; }

    /// <summary>
    /// The time to wait after connecting a socket before sending messages. Can be used for API's which will rate limit if you subscribe directly after connecting.
    /// </summary>
    public TimeSpan DelayAfterConnect { get; set; } = TimeSpan.Zero;

    public WebSocketApiClientOptions() : this(string.Empty) { }
    public WebSocketApiClientOptions(string baseAddress)
    {
        // Base Address
        this.BaseAddress = baseAddress;

        // Encoding
        this.Encoding = Encoding.UTF8;

        // Json Options
        this.JsonOptions = new JsonOptions
        {
            ErrorBehavior = ErrorBehavior.ThrowException,
        };
    }
}
