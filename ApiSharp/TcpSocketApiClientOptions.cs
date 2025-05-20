namespace ApiSharp;

/// <summary>
/// TCP Socket API Client Options
/// </summary>
public class TcpSocketApiClientOptions : BaseClientOptions
{
    /// <summary>
    /// Header Bytes
    /// </summary>
    public byte[] HeaderBytes = [0xF1, 0xF2];

    /// <summary>
    /// Socket Security
    /// </summary>
    public TcpSocketSecurity SocketSecurity { get; set; }

    /// <summary>
    /// Server Host
    /// </summary>
    public string ServerHost { get; set; } = "";

    /// <summary>
    /// Server Port
    /// </summary>
    public int ServerPort { get; set; }

    /// <summary>
    /// Heartbeat Enabled
    /// </summary>
    public bool HeartBeatEnabled { get; set; }

    /// <summary>
    /// Heartbeat Interval (in milliseconds)
    /// </summary>
    public int HeartBeatInterval { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public TcpSocketApiClientOptions() : this(string.Empty) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="baseAddress"></param>
    public TcpSocketApiClientOptions(string baseAddress)
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

        // Ping-Pong
        this.HeartBeatEnabled = false;
        this.HeartBeatInterval = 1000;
    }
}
