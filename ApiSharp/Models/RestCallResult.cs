namespace ApiSharp.Models;

/// <summary>
/// Rest Call Request
/// </summary>
/// <param name="url"></param>
/// <param name="method"></param>
/// <param name="body"></param>
/// <param name="headers"></param>
public class RestCallRequest(string? url, HttpMethod? method, string? body, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? headers)
{
    /// <summary>
    /// Request URL
    /// </summary>
    public string Url { get; set; } = url ?? "";

    /// <summary>
    /// Method
    /// </summary>
    public HttpMethod Method { get; set; } = method;

    /// <summary>
    /// Request Body
    /// </summary>
    public string Body { get; set; } = body ?? "";

    /// <summary>
    /// Request Headers
    /// </summary>
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; } = headers ?? [];
}

/// <summary>
/// Rest Call Response
/// </summary>
/// <param name="time"></param>
/// <param name="statusCode"></param>
/// <param name="headers"></param>
public class RestCallResponse(TimeSpan? time, HttpStatusCode? statusCode, IEnumerable<KeyValuePair<string, IEnumerable<string>>>? headers)
{
    /// <summary>
    /// Response Time
    /// </summary>
    public TimeSpan? ResponseTime { get; set; } = time;

    /// <summary>
    /// Response Status Code
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; } = statusCode;

    /// <summary>
    /// Response Headers
    /// </summary>
    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Headers { get; set; } = headers ?? [];
}

/// <summary>
/// Rest Call Result
/// </summary>
/// <param name="request"></param>
/// <param name="response"></param>
/// <param name="error"></param>
public class RestCallResult(RestCallRequest request, RestCallResponse response, Error error) : CallResult(error)
{
    /// <summary>
    /// Request
    /// </summary>
    public RestCallRequest Request { get; set; } = request;

    /// <summary>
    /// Response
    /// </summary>
    public RestCallResponse Response { get; set; } = response;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="responseCode"></param>
    /// <param name="responseHeaders"></param>
    /// <param name="responseTime"></param>
    /// <param name="requestUrl"></param>
    /// <param name="requestBody"></param>
    /// <param name="requestMethod"></param>
    /// <param name="requestHeaders"></param>
    /// <param name="error"></param>
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
            error)
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="error"></param>
    public RestCallResult(Error error) : this(null, null, error) { }

    /// <summary>
    /// Error Response
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public RestCallResult AsError(Error error)
    {
        return new RestCallResult(Request, Response, error);
    }
}

/// <summary>
/// Rest Call Result
/// </summary>
/// <typeparam name="T"></typeparam>
/// <param name="request"></param>
/// <param name="response"></param>
/// <param name="data"></param>
/// <param name="raw"></param>
/// <param name="error"></param>
public class RestCallResult<T>(RestCallRequest request, RestCallResponse response, T data, string raw, Error? error) : CallResult<T>(data, raw, error)
{
    /// <summary>
    /// Request
    /// </summary>
    public RestCallRequest Request { get; set; } = request;

    /// <summary>
    /// Response
    /// </summary>
    public RestCallResponse Response { get; set; } = response;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="responseCode"></param>
    /// <param name="responseHeaders"></param>
    /// <param name="responseTime"></param>
    /// <param name="responseRaw"></param>
    /// <param name="requestUrl"></param>
    /// <param name="requestBody"></param>
    /// <param name="requestMethod"></param>
    /// <param name="requestHeaders"></param>
    /// <param name="data"></param>
    /// <param name="error"></param>
    public RestCallResult(
        HttpStatusCode? responseCode,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? responseHeaders,
        TimeSpan? responseTime,
        string? responseRaw,
        string? requestUrl,
        string? requestBody,
        HttpMethod? requestMethod,
        IEnumerable<KeyValuePair<string, IEnumerable<string>>>? requestHeaders,
        T data,
        Error? error) : this(
            new RestCallRequest(requestUrl, requestMethod, requestBody, requestHeaders),
            new RestCallResponse(responseTime, responseCode, responseHeaders),
            data, responseRaw, error)
    { }

    public RestCallResult(Error error) : this(null, null, default, null, error) { }

    public RestCallResult(RestCallRequest request, RestCallResponse response, string raw, Error? error) : this(request, response, default, raw, error) { }

    /// <summary>
    /// Copy the RestCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="data">The data of the new type</param>
    /// <returns></returns>
    public new RestCallResult<K> As<K>(
#if NETSTANDARD2_1_OR_GREATER
        [AllowNull] 
#endif
        K data)
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
