namespace ApiSharp.Socket.Events
{
    public class OnClientDisconnectedEventArgs : EventArgs
    {
        public DisconnectReason Reason { get; internal set; }
    }
}
