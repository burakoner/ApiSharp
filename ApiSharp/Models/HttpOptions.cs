namespace ApiSharp.Models;

/// <summary>
/// HttpOptions
/// </summary>
public class HttpOptions
{
    /// <summary>
    /// User Agent
    /// </summary>
    public string UserAgent { get; set; } = "";

    /// <summary>
    /// Accept MIME Type
    /// </summary>
    public string AcceptMimeType { get; set; } = "";

    /// <summary>
    /// Request Timeout
    /// </summary>
    public TimeSpan RequestTimeout { get; set; }

    /// <summary>
    /// Encode Query String
    /// </summary>
    public bool EncodeQueryString { get; set; }
}