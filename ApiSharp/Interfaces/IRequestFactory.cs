namespace ApiSharp.Interfaces;

/// <summary>
/// Request factory interface
/// </summary>
public interface IRequestFactory
{
    /// <summary>
    /// Create a request for an uri
    /// </summary>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="requestId"></param>
    /// <returns></returns>
    IRequest Create(HttpMethod method, Uri uri, int requestId);

    /// <summary>
    /// Configure the requests created by this factory
    /// </summary>
    /// <param name="options">HttpClient Options</param>
    /// <param name="proxy">Proxy settings to use</param>       
    /// <param name="client">Optional shared http client instance</param>
    void Configure(HttpOptions options, ProxyCredentials proxy, HttpClient client = null);
}
