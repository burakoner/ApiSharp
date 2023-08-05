namespace ApiSharp.Events;

public class OnServerDisconnectedEventArgs : EventArgs
{
    public long ConnectionId { get; internal set; }
    public TcpSocketDisconnectReason Reason { get; internal set; }
}
