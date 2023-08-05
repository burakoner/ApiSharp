﻿namespace ApiSharp.WebSocket;

/// <summary>
/// Parameters for a websocket
/// </summary>
public class WebSocketParameters
{
    /// <summary>
    /// The uri to connect to
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// Headers to send in the connection handshake
    /// </summary>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Cookies to send in the connection handshake
    /// </summary>
    public IDictionary<string, string> Cookies { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// The time to wait between reconnect attempts
    /// </summary>
    public TimeSpan ReconnectInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Proxy for the connection
    /// </summary>
    public ProxyCredentials Proxy { get; set; }

    /// <summary>
    /// Whether the socket should automatically reconnect when connection is lost
    /// </summary>
    public bool AutoReconnect { get; set; }

    /// <summary>
    /// The maximum time of no data received before considering the connection lost and closting/reconnecting the socket
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Interval at which to send ping frames
    /// </summary>
    public TimeSpan? KeepAliveInterval { get; set; }

    /// <summary>
    /// The max amount of messages to send per second
    /// </summary>
    public int? RateLimitPerSecond { get; set; }

    /// <summary>
    /// Origin header value to send in the connection handshake
    /// </summary>
    public string Origin { get; set; }

    /// <summary>
    /// Delegate used for processing byte data received from socket connections before it is processed by handlers
    /// </summary>
    public Func<byte[], string> DataInterpreterBytes { get; set; }

    /// <summary>
    /// Delegate used for processing string data received from socket connections before it is processed by handlers
    /// </summary>
    public Func<string, string> DataInterpreterString { get; set; }

    /// <summary>
    /// Encoding for sending/receiving data
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="uri">Uri</param>
    /// <param name="autoReconnect">Auto reconnect</param>
    public WebSocketParameters(Uri uri, bool autoReconnect)
    {
        Uri = uri;
        AutoReconnect = autoReconnect;
    }
}
