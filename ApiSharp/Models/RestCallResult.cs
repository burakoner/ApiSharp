namespace ApiSharp.Models;


public class RestCallRequest
{
    public string Url { get; set; }
    public HttpMethod Method { get; set; }
    public string Body { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; }

    public RestCallRequest(string url, HttpMethod method, string body, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        Url = url;
        Method = method;
        Body = body;
        Headers = headers;
    }
}

public class RestCallResponse
{
    public TimeSpan? ResponseTime { get; set; }
    public HttpStatusCode? StatusCode { get; set; }
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; }

    public RestCallResponse(TimeSpan? time, HttpStatusCode? statusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        Headers = headers;
        ResponseTime = time;
        StatusCode = statusCode;
    }
}

public class RestCallResult : CallResult
{
    public RestCallRequest Request { get; set; }
    public RestCallResponse Response { get; set; }

    public RestCallResult(
        HttpStatusCode? responseCode,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
        TimeSpan? responseTime,
        string requestUrl,
        string requestBody,
        HttpMethod requestMethod,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
        Error error) : this(
            new RestCallRequest(requestUrl, requestMethod, requestBody, requestHeaders), 
            new RestCallResponse(responseTime, responseCode, responseHeaders), 
            error) { }

    public RestCallResult(Error error) : this(null, null, error) { }

    public RestCallResult(RestCallRequest request, RestCallResponse response, Error error) : base(error)
    {
        Request = request;
        Response = response;
    }

    public RestCallResult AsError(Error error)
    {
        return new RestCallResult(Request, Response, error);
    }
}

public class RestCallResult<T> : CallResult<T>
{
    public RestCallRequest Request { get; set; }
    public RestCallResponse Response { get; set; }

    public RestCallResult(
        HttpStatusCode? responseCode,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders,
        TimeSpan? responseTime,
        string responseRaw,
        string requestUrl,
        string requestBody,
        HttpMethod requestMethod,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders,
        T data,
        Error error) : this(
            new RestCallRequest(requestUrl, requestMethod, requestBody, requestHeaders),
            new RestCallResponse(responseTime, responseCode, responseHeaders), 
            data, responseRaw, error) { }

    public RestCallResult(Error error) : this(null, null, default, null, error) { }

    public RestCallResult(RestCallRequest request, RestCallResponse response, string raw, Error error) : this(request, response, default, raw, error) { }

    public RestCallResult(RestCallRequest request, RestCallResponse response, T data, string raw, Error error) : base(data, raw, error)
    {
        Request = request;
        Response = response;
    }

    /// <summary>
    /// Copy the RestCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="data">The data of the new type</param>
    /// <returns></returns>
    public new RestCallResult<K> As<K>([AllowNull] K data)
    {
        return new RestCallResult<K>(Request, Response, data, Raw, Error);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public RestCallResult AsDataless()
    {
        return new RestCallResult(Request, Response, Error);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public RestCallResult AsDatalessError(Error error)
    {
        return new RestCallResult(Request, Response, error);
    }

    /// <summary>
    /// Copy the WebCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="error">The error returned</param>
    /// <returns></returns>
    public new RestCallResult<K> AsError<K>(Error error)
    {
        return new RestCallResult<K>(Request, Response, default, Raw, error);
    }
}
