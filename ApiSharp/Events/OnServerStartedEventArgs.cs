namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when the server has started.
/// </summary>
/// <remarks>This event argument contains information about the server's start state.</remarks>
public class OnServerStartedEventArgs : EventArgs
{
    /// <summary>
    /// Indicates whether the server has started successfully.
    /// </summary>
    public bool IsStarted { get; internal set; }
}
