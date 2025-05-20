namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a client disconnects from a TCP socket.
/// </summary>
/// <remarks>This event argument contains information about the reason for the disconnection,  which can be used
/// to determine the cause of the client's disconnection.</remarks>
public class OnClientDisconnectedEventArgs : EventArgs
{
    /// <summary>
    /// Disconnect reason.
    /// </summary>
    public TcpSocketDisconnectReason Reason { get; internal set; }
}
