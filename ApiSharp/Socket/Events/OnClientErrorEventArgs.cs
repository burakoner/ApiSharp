namespace ApiSharp.Socket.Events
{
    public class OnClientErrorEventArgs : EventArgs
    {
        public Exception Exception { get; internal set; }
    }
}
