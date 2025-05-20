namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a server stops.
/// </summary>
/// <remarks>This event argument contains information about the server's stopped state.</remarks>
public class OnServerStoppedEventArgs : EventArgs
{
    /// <summary>
    /// Indicates whether the server has stopped.
    /// </summary>
    public bool IsStopped { get; internal set; }
}
