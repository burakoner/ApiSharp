namespace ApiSharp.Enums;

/// <summary>
/// TCP Socket Disconnect Reason
/// </summary>
public enum TcpSocketDisconnectReason
{
    /// <summary>
    /// No reason specified. This is the default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// Exception occurred. This value is used when an exception occurs during the operation of the TCP socket.
    /// </summary>
    Exception = 1,

    /// <summary>
    /// Server aborted. This value is used when the server aborts the connection.
    /// </summary>
    ServerAborted = 2,

    /// <summary>
    /// Server stopped. This value is used when the server stops the connection.
    /// </summary>
    ServerStopped = 3,
}
