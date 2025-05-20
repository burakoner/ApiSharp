namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event triggered when a server receives data from a client.
/// </summary>
/// <remarks>This event argument contains information about the client that sent the data, the connection
/// identifier, and the received data itself. It is typically used in event handlers to process incoming data from
/// clients.</remarks>
public class OnServerDataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the underlying <see cref="TcpClient"/> used for network communication.
    /// </summary>
    /// <remarks>This property is intended for internal use or advanced scenarios where direct access to the 
    /// <see cref="TcpClient"/> is required. Modifying the <see cref="TcpClient"/> directly may affect  the behavior of
    /// the containing class.</remarks>
    public TcpClient Client { get; internal set; } = default!;

    /// <summary>
    /// ConnectionId of the client.
    /// </summary>
    public long ConnectionId { get; internal set; }

    /// <summary>
    /// Data received from the client.
    /// </summary>
    public byte[] Data { get; internal set; } = [];
}