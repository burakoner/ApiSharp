﻿namespace ApiSharp.Stream;

/// <summary>
/// An update received from a stream update subscription
/// </summary>
/// <typeparam name="T">The type of the data</typeparam>
public class StreamDataEvent<T>
{
    /// <summary>
    /// The timestamp the data was received
    /// </summary>
    public DateTime Timestamp { get; set; }
    /// <summary>
    /// The topic of the update, what symbol/asset etc..
    /// </summary>
    public string Topic { get; set; }

    /// <summary>
    /// The original data that was received, only available when OutputRaw is set to true in the client options
    /// </summary>
    public string Raw { get; set; }
    /// <summary>
    /// The received data deserialized into an object
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="data"></param>
    /// <param name="timestamp"></param>
    public StreamDataEvent(T data, DateTime timestamp)
    {
        Data = data;
        Timestamp = timestamp;
    }

    internal StreamDataEvent(T data, string topic, DateTime timestamp)
    {
        Data = data;
        Topic = topic;
        Timestamp = timestamp;
    }

    internal StreamDataEvent(T data, string topic, string raw, DateTime timestamp)
    {
        Raw = raw;
        Data = data;
        Topic = topic;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Create a new DataEvent with data in the from of type K based on the current DataEvent. Topic, RawResponse and Timestamp will be copied over
    /// </summary>
    /// <typeparam name="K">The type of the new data</typeparam>
    /// <param name="data">The new data</param>
    /// <returns></returns>
    public StreamDataEvent<K> As<K>(K data)
    {
        return new StreamDataEvent<K>(data, Topic, Raw, Timestamp);
    }

    /// <summary>
    /// Create a new DataEvent with data in the from of type K based on the current DataEvent. RawResponse and Timestamp will be copied over
    /// </summary>
    /// <typeparam name="K">The type of the new data</typeparam>
    /// <param name="data">The new data</param>
    /// <param name="topic">The new topic</param>
    /// <returns></returns>
    public StreamDataEvent<K> As<K>(K data, string topic)
    {
        return new StreamDataEvent<K>(data, topic, Raw, Timestamp);
    }
}
