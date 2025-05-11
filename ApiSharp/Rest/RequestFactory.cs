namespace ApiSharp.Rest;

/// <summary>
/// Request Factory
/// </summary>
public class RequestFactory : IRequestFactory
{
    private HttpClient? httpClient;

    /// <summary>
    /// Configure the requests created by this factory
    /// </summary>
    /// <param name="options"></param>
    /// <param name="proxy"></param>
    /// <param name="client"></param>
    public void Configure(HttpOptions options, ProxyCredentials proxy, HttpClient? client = null)
    {
        if (client == null)
        {
            HttpMessageHandler handler = new HttpClientHandler()
            {
                Proxy = proxy == null ? null : new WebProxy
                {
                    Address = new Uri($"{proxy.Host}:{proxy.Port}"),
                    Credentials = proxy.Password == null ? null : new NetworkCredential(proxy.Username.GetString(), proxy.Password.GetString())
                }
            };

            httpClient = new HttpClient(handler);
            httpClient.Timeout = options.RequestTimeout;
            httpClient.DefaultRequestHeaders.Add("User-Agent", options.UserAgent);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        else
        {
            httpClient = client;
        }
    }

    /// <summary>
    /// Create a request for an uri
    /// </summary>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <param name="requestId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IRequest Create(HttpMethod method, Uri uri, int requestId)
    {
        if (httpClient == null)
            throw new InvalidOperationException("Cant create request before configuring http client");

        return new Request(new HttpRequestMessage(method, uri), httpClient, requestId);
    }
}
