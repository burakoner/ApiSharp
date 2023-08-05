namespace ApiSharp.WebSocket;

public enum WebSocketStatus
{
    /// <summary>
    /// None/Initial
    /// </summary>
    None,

    /// <summary>
    /// Connected
    /// </summary>
    Connected,

    /// <summary>
    /// Reconnecting
    /// </summary>
    Reconnecting,

    /// <summary>
    /// Resubscribing on reconnected socket
    /// </summary>
    Resubscribing,

    /// <summary>
    /// Closing
    /// </summary>
    Closing,

    /// <summary>
    /// Closed
    /// </summary>
    Closed,

    /// <summary>
    /// Disposed
    /// </summary>
    Disposed
}
