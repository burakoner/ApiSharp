namespace ApiSharp.Rest;

/// <summary>
/// Request object, wrapper for HttpRequestMessage
/// </summary>
public class Request : IRequest
{
    private readonly HttpRequestMessage request;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Construct a new request object
    /// </summary>
    /// <param name="request"></param>
    /// <param name="client"></param>
    /// <param name="requestId"></param>
    public Request(HttpRequestMessage request, HttpClient client, int requestId)
    {
        httpClient = client;
        this.request = request;
        RequestId = requestId;
    }

    /// <summary>
    /// Content of the request
    /// </summary>
    public string Content { get; private set; } = "";

    /// <summary>
    /// Accept header for the request, used to specify the expected response format
    /// </summary>
    public string Accept
    {
        set => request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
    }

    /// <summary>
    /// Method of the request, e.g. GET, POST, PUT, DELETE
    /// </summary>
    public HttpMethod Method
    {
        get => request.Method;
        set => request.Method = value;
    }

    /// <summary>
    /// Uri of the request, the endpoint to which the request is sent
    /// </summary>
    public Uri Uri => request.RequestUri;

    /// <summary>
    /// Request id for tracing purposes, can be used to track requests in logs or debugging
    /// </summary>
    public int RequestId { get; }

    /// <summary>
    /// Set the content of the request as a byte array
    /// </summary>
    /// <param name="data"></param>
    /// <param name="contentType"></param>
    public void SetContent(string data, string contentType)
    {
        Content = data;
        request.Content = new StringContent(data, Encoding.UTF8, contentType);
    }

    /// <summary>
    /// Add a header to the request
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void AddHeader(string key, string value)
    {
        // request.Headers.Add(key, value);
        request.Headers.TryAddWithoutValidation(key, value);
    }

    /// <summary>
    /// Get all headers from the request
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, IEnumerable<string>> GetHeaders()
    {
        return request.Headers.ToDictionary(h => h.Key, h => h.Value);
    }

    /// <summary>
    /// Set the content of the request as a byte array
    /// </summary>
    /// <param name="data"></param>
    public void SetContent(byte[] data)
    {
        request.Content = new ByteArrayContent(data);
    }

    /// <summary>
    /// Get the response for this request asynchronously
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IResponse> GetResponseAsync(CancellationToken cancellationToken)
    {
        return new Response(await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false));
    }
}
