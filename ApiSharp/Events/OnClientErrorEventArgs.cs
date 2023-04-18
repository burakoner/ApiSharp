namespace ApiSharp.Events;

public class OnClientErrorEventArgs : EventArgs
{
    public Exception Exception { get; internal set; }
}
