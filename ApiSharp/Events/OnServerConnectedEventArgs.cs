namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a server connection is established.
/// </summary>
/// <remarks>This event argument contains information about the connected server, including its IP address, port,
/// and a unique connection identifier.</remarks>
public class OnServerConnectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="System.Net.IPEndPoint"/> associated with the current connection.
    /// </summary>
    public IPEndPoint IPEndPoint { get; internal set; } = default!;

    /// <summary>
    /// IPAddress of the server.
    /// </summary>
    public string IPAddress { get; internal set; } = "";

    /// <summary>
    /// Port of the server.
    /// </summary>
    public int Port { get; internal set; }

    /// <summary>
    /// ConnectionId of the server.
    /// </summary>
    public long ConnectionId { get; internal set; }
}
