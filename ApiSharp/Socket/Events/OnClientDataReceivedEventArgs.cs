namespace ApiSharp.Socket.Events
{
    public class OnClientDataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; internal set; }
    }
}
