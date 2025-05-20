namespace ApiSharp.Events;

/// <summary>
/// Provides data for the server error event, including details about the client connection and the exception that
/// occurred.
/// </summary>
/// <remarks>This event argument is typically used to handle errors that occur during server-client communication.
/// It provides information about the client connection and the specific exception that was raised.</remarks>
public class OnServerErrorEventArgs : EventArgs
{
    /// <summary>
    /// Client that caused the error.
    /// </summary>
    public TcpClient Client { get; internal set; }=default!;

    /// <summary>
    /// ConnectionId of the client.
    /// </summary>
    public long ConnectionId { get; internal set; }

    /// <summary>
    /// Exception that caused the error.
    /// </summary>
    public Exception Exception { get; internal set; } = default!;
}
