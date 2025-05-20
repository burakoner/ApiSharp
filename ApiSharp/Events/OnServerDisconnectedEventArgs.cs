namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a server connection is disconnected.
/// </summary>
/// <remarks>This class contains information about the disconnection, including the connection ID and the reason
/// for the disconnection. Instances of this class are typically used in event handlers to process disconnection
/// events.</remarks>
public class OnServerDisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// ConnectionId of the server.
    /// </summary>
    public long ConnectionId { get; internal set; }

    /// <summary>
    /// Disconnect reason.
    /// </summary>
    public TcpSocketDisconnectReason Reason { get; internal set; }
}
