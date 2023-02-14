namespace ApiSharp.Models;

public class HttpOptions
{
    public string UserAgent { get; set; }
    public string AcceptMimeType { get; set; }
    public TimeSpan RequestTimeout { get; set; }
    public bool EncodeQueryString { get; set; }
    public RestBodyFormat BodyFormat { get; set; }
}