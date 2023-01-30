namespace ApiSharp.Rest;

public class Request : IRequest
{
    private readonly HttpRequestMessage request;
    private readonly HttpClient httpClient;

    public Request(HttpRequestMessage request, HttpClient client, int requestId)
    {
        httpClient = client;
        this.request = request;
        RequestId = requestId;
    }

    public string Content { get; private set; }

    public string Accept
    {
        set => request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
    }

    public HttpMethod Method
    {
        get => request.Method;
        set => request.Method = value;
    }

    public Uri Uri => request.RequestUri;

    public int RequestId { get; }

    public void SetContent(string data, string contentType)
    {
        Content = data;
        request.Content = new StringContent(data, Encoding.UTF8, contentType);
    }

    public void AddHeader(string key, string value)
    {
        request.Headers.Add(key, value);
    }

    public Dictionary<string, IEnumerable<string>> GetHeaders()
    {
        return request.Headers.ToDictionary(h => h.Key, h => h.Value);
    }

    public void SetContent(byte[] data)
    {
        request.Content = new ByteArrayContent(data);
    }

    public async Task<IResponse> GetResponseAsync(CancellationToken cancellationToken)
    {
        return new Response(await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false));
    }
}
