﻿namespace ApiSharp.WebSocket;

/// <summary>
/// Received message info
/// </summary>
public struct WebSocketReceiveItem
{
    /// <summary>
    /// Timestamp of the received data
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Number of bytes received
    /// </summary>
    public int Bytes { get; set; }

    /// <summary>
    /// Construct a new WebSocketReceiveItem
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="bytes"></param>
    public WebSocketReceiveItem(DateTime timestamp, int bytes)
    {
        Timestamp = timestamp;
        Bytes = bytes;
    }
}
