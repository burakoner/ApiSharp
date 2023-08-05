namespace ApiSharp;

public class TcpSocketApiClientOptions : BaseClientOptions
{
    // Header
    public byte[] HeaderBytes = new byte[] { 0xF1, 0xF2 };

    // Security
    public TcpSocketSecurity SocketSecurity { get; set; }

    // TCP Server
    public string ServerHost { get; set; }
    public int ServerPort { get; set; }

    // Ping-Pong
    public bool HeartBeatEnabled { get; set; }
    public int HeartBeatInterval { get; set; }

    public TcpSocketApiClientOptions() : this(string.Empty) { }
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
