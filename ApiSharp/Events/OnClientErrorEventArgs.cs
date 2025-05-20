namespace ApiSharp.Events;

/// <summary>
/// Provides data for the event that is raised when a client error occurs.
/// </summary>
/// <remarks>This class contains information about the error that occurred, including the exception that caused
/// the error. It is typically used in event handlers to inspect the error details and take appropriate
/// action.</remarks>
public class OnClientErrorEventArgs : EventArgs
{
    /// <summary>
    /// Exception that caused the error.
    /// </summary>
    public Exception Exception { get; internal set; } = null!;
}
