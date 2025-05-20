namespace ApiSharp.Events;

/// <summary>
/// Provides data for the server connection request event.
/// </summary>
/// <remarks>This event argument contains information about an incoming connection request to the server, 
/// including the IP address, port, and whether the connection should be accepted.</remarks>
public class OnServerConnectionRequestEventArgs : EventArgs
{
    /// <summary>
    /// Gets the <see cref="System.Net.IPEndPoint"/> associated with the current connection.
    /// </summary>
    public IPEndPoint IPEndPoint { get; internal set; }=default!;

    /// <summary>
    /// IPAddress of the server.
    /// </summary>
    public string IPAddress { get; internal set; }="";

    /// <summary>
    /// Port of the server.
    /// </summary>
    public int Port { get; internal set; }

    /// <summary>
    /// Accept or reject the connection request.
    /// </summary>
    public bool Accept { get; set; } = true;
}
