namespace ApiSharp.Enums;

/// <summary>
/// TCP Socket Security
/// </summary>
public enum TcpSocketSecurity
{
    /// <summary>
    /// No security. This is the default value.
    /// </summary>
    None = 0,

    /// <summary>
    /// CRC16 security. This value is used when CRC16 security is applied to the TCP socket.
    /// </summary>
    CRC16 = 1,

    /// <summary>
    /// CRC32 security. This value is used when CRC32 security is applied to the TCP socket.
    /// </summary>
    CRC32 = 2,
}
