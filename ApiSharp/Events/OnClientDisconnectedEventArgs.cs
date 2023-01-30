namespace ApiSharp.Events
{
    public class OnClientDisconnectedEventArgs : EventArgs
    {
        public SocketDisconnectReason Reason { get; internal set; }
    }
}
