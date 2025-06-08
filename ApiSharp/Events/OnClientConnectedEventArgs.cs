namespace ApiSharp.Events;

/// <summary>
/// OnClientConnectedEventArgs
/// </summary>
public class OnClientConnectedEventArgs : EventArgs
{
    /// <summary>
    /// ServerIPAddress
    /// </summary>
    public IPAddress ServerIPAddress
    {
        get { return IPAddress.Parse(ServerHost); }
    }

    /// <summary>
    /// ServerHost
    /// </summary>
    public string ServerHost { get; internal set; } = "";

    /// <summary>
    /// ServerPort
    /// </summary>
    public int ServerPort { get; internal set; }
}
