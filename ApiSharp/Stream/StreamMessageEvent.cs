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
    public string OriginalData { get; set; }

    /// <summary>
    /// The timestamp of when the data was received
    /// </summary>
    public DateTime ReceivedTimestamp { get; set; }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="jsonData"></param>
    /// <param name="originalData"></param>
    /// <param name="timestamp"></param>
    public StreamMessageEvent(StreamConnection connection, JToken jsonData, string originalData, DateTime timestamp)
    {
        Connection = connection;
        JsonData = jsonData;
        OriginalData = originalData;
        ReceivedTimestamp = timestamp;
    }
}
