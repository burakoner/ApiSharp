namespace ApiSharp.WebSocket;

/// <summary>
/// Received message info
/// </summary>
public struct WebSocketReceiveItem
{
    /// <summary>
    /// Timestamp of the received data
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Number of bytes received
    /// </summary>
    public int Bytes { get; set; }

    public WebSocketReceiveItem(DateTime timestamp, int bytes)
    {
        Timestamp = timestamp;
        Bytes = bytes;
    }
}
