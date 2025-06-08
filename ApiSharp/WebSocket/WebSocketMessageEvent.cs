namespace ApiSharp.WebSocket;

/// <summary>
/// WebSocketMessageEvent
/// </summary>
/// <param name="connection"></param>
/// <param name="jsonData"></param>
/// <param name="raw"></param>
/// <param name="timestamp"></param>
public class WebSocketMessageEvent(WebSocketConnection connection, JToken jsonData, string? raw, DateTime timestamp)
{
    /// <summary>
    /// The connection the message was received on
    /// </summary>
    public WebSocketConnection Connection { get; set; } = connection;

    /// <summary>
    /// The json object of the data
    /// </summary>
    public JToken JsonData { get; set; } = jsonData;

    /// <summary>
    /// The originally received string data
    /// </summary>
    public string? Raw { get; set; } = raw;

    /// <summary>
    /// The timestamp of when the data was received
    /// </summary>
    public DateTime ReceivedTimestamp { get; set; } = timestamp;
}
