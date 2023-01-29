namespace ApiSharp.Models;

public class RestCallResult : CallResult
{
    public string RequestUrl { get; set; }
    public HttpMethod RequestMethod { get; set; }
    public string RequestBody { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> RequestHeaders { get; set; }

    public TimeSpan? ResponseTime { get; set; }
    public HttpStatusCode? ResponseStatusCode { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders { get; set; }

    public RestCallResult(
        HttpStatusCode? code,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
        TimeSpan? responseTime,
        string requestUrl,
        string requestBody,
        HttpMethod requestMethod,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
        CallError error) : base(error)
    {
        ResponseStatusCode = code;
        ResponseHeaders = responseHeaders;
        ResponseTime = responseTime;

        RequestUrl = requestUrl;
        RequestBody = requestBody;
        RequestHeaders = requestHeaders;
        RequestMethod = requestMethod;
    }

    public RestCallResult(CallError error) : base(error) { }

    public RestCallResult AsError(CallError error)
    {
        return new RestCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
    }
}

public class RestCallResult<T> : CallResult<T>
{
    public string RequestUrl { get; set; }
    public HttpMethod RequestMethod { get; set; }
    public string RequestBody { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> RequestHeaders { get; set; }

    public TimeSpan? ResponseTime { get; set; }
    public HttpStatusCode? ResponseStatusCode { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> ResponseHeaders { get; set; }

    public RestCallResult(
        HttpStatusCode? code,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
        TimeSpan? responseTime,
        string originalData,
        string requestUrl,
        string requestBody,
        HttpMethod requestMethod,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
        T data,
        CallError error) : base(data, originalData, error)
    {
        ResponseStatusCode = code;
        ResponseHeaders = responseHeaders;
        ResponseTime = responseTime;

        RequestUrl = requestUrl;
        RequestBody = requestBody;
        RequestHeaders = requestHeaders;
        RequestMethod = requestMethod;
    }

    public RestCallResult(CallError error) : this(null, null, null, null, null, null, null, null, default, error) { }

    /// <summary>
    /// Copy the RestCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="data">The data of the new type</param>
    /// <returns></returns>
    public new RestCallResult<K> As<K>([AllowNull] K data)
    {
        return new RestCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, OriginalData, RequestUrl, RequestBody, RequestMethod, RequestHeaders, data, Error);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public RestCallResult AsDataless()
    {
        return new RestCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, Error);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public RestCallResult AsDatalessError(CallError error)
    {
        return new RestCallResult(ResponseStatusCode, ResponseHeaders, ResponseTime, RequestUrl, RequestBody, RequestMethod, RequestHeaders, error);
    }

    /// <summary>
    /// Copy the WebCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="error">The error returned</param>
    /// <returns></returns>
    public new RestCallResult<K> AsError<K>(CallError error)
    {
        return new RestCallResult<K>(ResponseStatusCode, ResponseHeaders, ResponseTime, OriginalData, RequestUrl, RequestBody, RequestMethod, RequestHeaders, default, error);
    }
}
