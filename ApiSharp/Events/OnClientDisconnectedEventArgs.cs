namespace ApiSharp.Events;

public class OnClientDisconnectedEventArgs : EventArgs
{
    public TcpSocketDisconnectReason Reason { get; internal set; }
}
