namespace ApiSharp.Stream;

public class StreamMessageEvent
{
    /// <summary>
    /// The connection the message was received on
    /// </summary>
    public StreamConnection Connection { get; set; }

    /// <summary>
    /// The json object of the data
    /// </summary>
    public JToken JsonData { get; set; }

    /// <summary>
    /// The originally received string data
    /// </summary>
    public string Raw { get; set; }

    /// <summary>
    /// The timestamp of when the data was received
    /// </summary>
    public DateTime ReceivedTimestamp { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="jsonData"></param>
    /// <param name="raw"></param>
    /// <param name="timestamp"></param>
    public StreamMessageEvent(StreamConnection connection, JToken jsonData, string raw, DateTime timestamp)
    {
        Connection = connection;
        JsonData = jsonData;
        Raw = raw;
        ReceivedTimestamp = timestamp;
    }
}
