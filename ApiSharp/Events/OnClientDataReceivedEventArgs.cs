namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a client receives data.
/// </summary>
/// <remarks>This event argument contains the data received from the client as a byte array.</remarks>
public class OnClientDataReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Data received from the client.
    /// </summary>
    public byte[] Data { get; internal set; } = [];
}
