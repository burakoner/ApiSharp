namespace ApiSharp.Events
{
    public class OnServerDisconnectedEventArgs : EventArgs
    {
        public long ConnectionId { get; internal set; }
        public SocketDisconnectReason Reason { get; internal set; }
    }
}
