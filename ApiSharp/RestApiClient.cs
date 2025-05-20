namespace ApiSharp;

/// <summary>
/// Rest API Base Client
/// </summary>
public abstract class RestApiClient : BaseClient
{
    /// <summary>
    /// The factory for creating requests. Used for unit testing
    /// </summary>
    protected IRequestFactory RequestFactory { get; set; } = new RequestFactory();

    /// <summary>
    /// Request headers to be sent with each request
    /// </summary>
    protected Dictionary<string, string>? StandardRequestHeaders { get; set; }

    /// <summary>
    /// Total amount of requests made with this API client
    /// </summary>
    protected int TotalRequestsMade { get; set; }

    /// <summary>
    /// Request body content type
    /// </summary>
    protected RestRequestBodyFormat RequestBodyFormat = RestRequestBodyFormat.Json;

    /// <summary>
    /// Whether or not we need to manually parse an error instead of relying on the http status code
    /// </summary>
    protected bool ManualParseError = false;

    /// <summary>
    /// How to serialize array parameters when making requests
    /// </summary>
    protected ArraySerialization ArraySerialization = ArraySerialization.Array;

    /// <summary>
    /// Client Options
    /// </summary>
    public RestApiClientOptions ClientOptions { get { return (RestApiClientOptions)base._options; } }

    /// <summary>
    /// Constructor
    /// </summary>
    protected RestApiClient() : this(null, new())
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="options"></param>
    protected RestApiClient(ILogger? logger, RestApiClientOptions options) : base(logger, options ?? new())
    {
        options ??= new RestApiClientOptions();
        RequestFactory.Configure(options.HttpOptions, options.Proxy, options.HttpClient);
    }

    /// <summary>
    /// Get time sync info for an API client
    /// </summary>
    protected internal virtual TimeSyncInfo GetTimeSyncInfo()
        => new(_logger, false, TimeSpan.MaxValue, new TimeSyncState(""));

    /// <summary>
    /// Get time offset for an API client
    /// </summary>
    protected internal virtual TimeSpan GetTimeOffset()
        => TimeSpan.Zero;

    /// <summary>
    /// Sends Request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <param name="method"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="signed"></param>
    /// <param name="queryParameters"></param>
    /// <param name="bodyParameters"></param>
    /// <param name="headerParameters"></param>
    /// <param name="serialization"></param>
    /// <param name="deserializer"></param>
    /// <param name="ignoreRatelimit"></param>
    /// <param name="requestWeight"></param>
    /// <returns></returns>
    protected virtual async Task<RestCallResult<T>> SendRequestAsync<T>(
        Uri uri,
        HttpMethod method,
        CancellationToken cancellationToken,
        bool signed = false,
        Dictionary<string, object>? queryParameters = null,
        Dictionary<string, object>? bodyParameters = null,
        Dictionary<string, string>? headerParameters = null,
        ArraySerialization? serialization = null,
        JsonSerializer? deserializer = null,
        bool ignoreRatelimit = false,
        int requestWeight = 1) // where T : class
    {
        var request = await PrepareRequestAsync(uri, method, cancellationToken, signed, queryParameters, bodyParameters, headerParameters, serialization, deserializer, ignoreRatelimit, requestWeight).ConfigureAwait(false);
        if (!request) return new RestCallResult<T>(request.Error!);

        return await GetResponseAsync<T>(request.Data, deserializer, cancellationToken, false).ConfigureAwait(false);
    }

    /// <summary>
    /// Prepares a request to be sent to the server
    /// </summary>
    /// <param name="uri">The uri to send the request to</param>
    /// <param name="method">The method of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="signed">Whether or not the request should be authenticated</param>
    /// <param name="queryParameters">The query string parameters of the request</param>
    /// <param name="bodyParameters">The body content parameters of the request</param>
    /// <param name="headerParameters">Additional headers to send with the request</param>
    /// <param name="serialization">How array parameters should be serialized, overwrites the value set in the client</param>
    /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
    /// <param name="ignoreRatelimit">Ignore rate limits for this request</param>
    /// <param name="requestWeight">Credits used for the request</param>
    protected virtual async Task<CallResult<IRequest>> PrepareRequestAsync(
        Uri uri,
        HttpMethod method,
        CancellationToken cancellationToken,
        bool signed = false,
        Dictionary<string, object>? queryParameters = null,
        Dictionary<string, object>? bodyParameters = null,
        Dictionary<string, string>? headerParameters = null,
        ArraySerialization? serialization = null,
        JsonSerializer? deserializer = null,
        bool ignoreRatelimit = false,
        int requestWeight = 1)
    {
        var requestId = NextId();

        if (signed)
        {
            var syncTask = SyncTimeAsync();
            var timeSyncInfo = GetTimeSyncInfo();
            if (timeSyncInfo.TimeSyncState.LastSyncTime == default)
            {
                // Initially with first request we'll need to wait for the time syncing, if it's not the first request we can just continue
                var syncTimeResult = await syncTask.ConfigureAwait(false);
                if (!syncTimeResult)
                {
                    _logger.Log(LogLevel.Debug, $"[{requestId}] Failed to sync time, aborting request: " + syncTimeResult.Error);
                    return syncTimeResult.As<IRequest>(default);
                }
            }
        }

        if (!ignoreRatelimit)
        {
            foreach (var limiter in ClientOptions.RateLimiters)
            {
                var limitResult = await limiter.LimitRequestAsync(_logger, uri.AbsolutePath, method, signed, ClientOptions.ApiCredentials?.Key, ClientOptions.RateLimitingBehavior, requestWeight, cancellationToken).ConfigureAwait(false);
                if (!limitResult.Success)
                    return new CallResult<IRequest>(limitResult.Error!);
            }
        }

        if (signed && AuthenticationProvider == null)
        {
            _logger.Log(LogLevel.Warning, $"[{requestId}] Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
            return new CallResult<IRequest>(new NoApiCredentialsError());
        }

        _logger.Log(LogLevel.Information, $"[{requestId}] Creating request for " + uri);
        var request = ConstructRequest(uri, method, signed, queryParameters, bodyParameters, headerParameters, serialization ?? this.ArraySerialization, requestId);

        var paramString = "";
        if (!string.IsNullOrWhiteSpace(request.Content))
            paramString = $" with request body '{request.Content}'";

        var headers = request.GetHeaders();
        if (headers.Any())
            paramString += " with headers " + string.Join(", ", headers.Select(h => h.Key + $"=[{string.Join(",", h.Value)}]"));

        TotalRequestsMade++;
        _logger.Log(LogLevel.Trace, $"[{requestId}] Sending {method}{(signed ? " signed" : "")} request to {request.Uri}{paramString ?? " "}{(ClientOptions.Proxy == null ? "" : $" via proxy {ClientOptions.Proxy.Host}")}");
        return new CallResult<IRequest>(request, request.Content);
    }

    /// <summary>
    /// Executes the request and returns the result deserialized into the type parameter class
    /// </summary>
    /// <param name="request">The request object to execute</param>
    /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="expectedEmptyResponse">If an empty response is expected</param>
    protected virtual async Task<RestCallResult<T>> GetResponseAsync<T>(
        IRequest request,
        JsonSerializer? deserializer,
        CancellationToken cancellationToken,
        bool expectedEmptyResponse)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            var response = await request.GetResponseAsync(cancellationToken).ConfigureAwait(false);
            sw.Stop();
            var statusCode = response.StatusCode;
            var headers = response.ResponseHeaders;
            var responseStream = await response.GetResponseStreamAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                // If we have to manually parse error responses (can't rely on HttpStatusCode) we'll need to read the full
                // response before being able to deserialize it into the resulting type since we don't know if it an error response or data
                if (ManualParseError)
                {
                    using var reader = new StreamReader(responseStream);
                    var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    responseStream.Close();
                    response.Close();
                    _logger.Log(LogLevel.Debug, $"[{request.RequestId}] Response received in {sw.ElapsedMilliseconds}ms: " + data);

                    if (!expectedEmptyResponse)
                    {
                        // Validate if it is valid json. Sometimes other data will be returned, 502 error html pages for example
                        var parseResult = ValidateJson(data);
                        if (!parseResult.Success)
                            return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, parseResult.Error!);

                        // Let the library implementation see if it is an error response, and if so parse the error
                        var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                        if (error != null)
                            return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error!);

                        // Not an error, so continue deserializing
                        var deserializeResult = Deserialize<T>(parseResult.Data, deserializer, request.RequestId);
                        return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), deserializeResult.Data, deserializeResult.Error);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data))
                        {
                            var parseResult = ValidateJson(data);
                            if (!parseResult.Success)
                                // Not empty, and not json
                                return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, parseResult.Error!);

                            var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                            if (error != null)
                                // Error response
                                return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error!);
                        }

                        // Empty success response; okay
                        return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, ClientOptions.RawResponse ? data : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, default);
                    }
                }
                else
                {
                    if (expectedEmptyResponse)
                    {
                        // We expected an empty response and the request is successful and don't manually parse errors, so assume it's correct
                        responseStream.Close();
                        response.Close();

                        return new RestCallResult<T>(statusCode, headers, sw.Elapsed, "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, null);
                    }

                    // Success status code, and we don't have to check for errors. Continue deserializing directly from the stream
                    var desResult = await DeserializeAsync<T>(responseStream, deserializer, request.RequestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
                    responseStream.Close();
                    response.Close();

                    return new RestCallResult<T>(statusCode, headers, sw.Elapsed, ClientOptions.RawResponse ? desResult.Raw : "", request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), desResult.Data, desResult.Error);
                }
            }
            else
            {
                // Http status code indicates error
                using var reader = new StreamReader(responseStream);
                var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                _logger.Log(LogLevel.Warning, $"[{request.RequestId}] Error received in {sw.ElapsedMilliseconds}ms: {data}");
                responseStream.Close();
                response.Close();
                var parseResult = ValidateJson(data);
                var error = parseResult.Success ? ParseErrorResponse(parseResult.Data) : new ServerError(data)!;
                if (error.Code == null || error.Code == 0)
                    error.Code = (int)response.StatusCode;
                return new RestCallResult<T>(statusCode, headers, sw.Elapsed, data, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error);
            }
        }
        catch (HttpRequestException requestException)
        {
            // Request exception, can't reach server for instance
            var exceptionInfo = requestException.ToLogString();
            _logger.Log(LogLevel.Warning, $"[{request.RequestId}] Request exception: " + exceptionInfo);
            return new RestCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new WebError(exceptionInfo));
        }
        catch (OperationCanceledException canceledException)
        {
            if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
            {
                // Cancellation token canceled by caller
                _logger.Log(LogLevel.Warning, $"[{request.RequestId}] Request canceled by cancellation token");
                return new RestCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new CancellationRequestedError());
            }
            else
            {
                // Request timed out
                _logger.Log(LogLevel.Warning, $"[{request.RequestId}] Request timed out: " + canceledException.ToLogString());
                return new RestCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new WebError($"[{request.RequestId}] Request timed out"));
            }
        }
    }

    /// <summary>
    /// Can be used to parse an error even though response status indicates success. Some apis always return 200 OK, even though there is an error.
    /// When setting manualParseError to true this method will be called for each response to be able to check if the response is an error or not.
    /// If the response is an error this method should return the parsed error, else it should return null
    /// </summary>
    /// <param name="data">Received data</param>
    /// <returns>Null if not an error, Error otherwise</returns>
    protected virtual Task<ServerError> TryParseErrorAsync(JToken data)
    {
        return Task.FromResult<ServerError>(null);
    }

    /// <summary>
    /// Creates a request object
    /// </summary>
    /// <param name="uri">The uri to send the request to</param>
    /// <param name="method">The method of the request</param>
    /// <param name="signed">Whether or not the request should be authenticated</param>
    /// <param name="queryParameters">The query string parameters of the request</param>
    /// <param name="bodyParameters">The body content parameters of the request</param>
    /// <param name="headerParameters">Additional headers to send with the request</param>
    /// <param name="serialization">How array parameters should be serialized</param>
    /// <param name="requestId">Unique id of a request</param>
    protected virtual IRequest ConstructRequest(
        Uri uri,
        HttpMethod method,
        bool signed,
        Dictionary<string, object>? queryParameters,
        Dictionary<string, object>? bodyParameters,
        Dictionary<string, string>? headerParameters,
        ArraySerialization serialization,
        int requestId
        )
    {
        queryParameters ??= [];
        bodyParameters ??= [];
        headerParameters ??= [];

        for (var i = 0; i < queryParameters.Count; i++)
        {
            var kvp = queryParameters.ElementAt(i);
            if (kvp.Value is Func<object> delegateValue)
                queryParameters[kvp.Key] = delegateValue();
        }

        for (var i = 0; i < bodyParameters.Count; i++)
        {
            var kvp = bodyParameters.ElementAt(i);
            if (kvp.Value is Func<object> delegateValue)
                bodyParameters[kvp.Key] = delegateValue();
        }

        foreach (var parameter in queryParameters)
        {
            uri = uri.AddQueryParmeter(parameter.Key, parameter.Value.ToString());
        }

        var parameters = new List<string>();
        headerParameters.ToList().ForEach(x => parameters.Add(x.Key));
        queryParameters.ToList().ForEach(x => parameters.Add(x.Key));
        bodyParameters.ToList().ForEach(x => parameters.Add(x.Key));

        var sortedHeaderParameters = new SortedDictionary<string, string>(headerParameters);
        var sortedQueryParameters = new SortedDictionary<string, object>(queryParameters);
        var sortedBodyParameters = new SortedDictionary<string, object>(bodyParameters);
        var sortedBodyContent = PrepareBodyContent(sortedBodyParameters, RequestBodyFormat);
        AuthenticationProvider?.AuthenticateRestApi(this, uri, method, signed, serialization, sortedQueryParameters, sortedBodyParameters, sortedBodyContent, sortedHeaderParameters);
        sortedBodyContent = PrepareBodyContent(sortedBodyParameters, RequestBodyFormat);

        // Sanity check
        foreach (var param in parameters)
        {
            if (!sortedQueryParameters.ContainsKey(param) && !sortedBodyParameters.ContainsKey(param) && !sortedHeaderParameters.ContainsKey(param))
            {
                throw new Exception($"Missing parameter {param} after authentication processing. " +
                    $"AuthenticationProvider implementation should return provided parameters in either the uri or body or header parameters output");
            }
        }

        // Add the auth parameters to the uri, start with a new URI to be able to sort the parameters including the auth parameters            
        uri = uri.SetParameters(sortedQueryParameters, serialization);

        var request = RequestFactory.Create(method, uri, requestId);
        request.Accept = RestApiConstants.JSON_CONTENT_HEADER;

        foreach (var header in sortedHeaderParameters)
        {
            request.AddHeader(header.Key, header.Value);
        }

        if (StandardRequestHeaders != null)
        {
            foreach (var header in StandardRequestHeaders)
            {
                // Only add it if it isn't overwritten
                if (headerParameters?.ContainsKey(header.Key) != true)
                    request.AddHeader(header.Key, header.Value);
            }
        }

        var contentType = RequestBodyFormat == RestRequestBodyFormat.Json
            ? RestApiConstants.JSON_CONTENT_HEADER
            : RestApiConstants.FORM_CONTENT_HEADER;
        if (sortedBodyParameters.Any())
        {
            request.SetContent(sortedBodyContent, contentType);
        }
        else
        {
            if (ClientOptions.SetRequestBodyEmptyContentMethods.Contains(method))
            {
                request.SetContent(RestApiConstants.RequestBodyEmptyContent, contentType);
            }
        }

        return request;
    }

    /// <summary>
    /// Prepares the parameters of the request to the request object body
    /// </summary>
    /// <param name="parameters">The parameters to set</param>
    /// <param name="format">Rest Request Body Format</param>
    protected virtual string PrepareBodyContent(SortedDictionary<string, object> parameters, RestRequestBodyFormat format)
    {
        var stringData = RestApiConstants.RequestBodyEmptyContent;
        if (parameters == null || !parameters.Any()) return stringData;

        if (format == RestRequestBodyFormat.Json)
        {
            // Write the parameters as json in the body
            stringData = JsonConvert.SerializeObject(parameters, SerializerOptions.WithConverters);
        }
        else if (format == RestRequestBodyFormat.FormData)
        {
            // Write the parameters as form data in the body
            stringData = parameters.ToFormData();
        }

        if (format == RestRequestBodyFormat.Json)
        {
            if (!string.IsNullOrWhiteSpace(RestApiConstants.RequestBodyParameterKey) && parameters.Count == 1 && parameters.Keys.First() == RestApiConstants.RequestBodyParameterKey)
            {
                // Write the parameters as json in the body
                stringData = JsonConvert.SerializeObject(parameters[RestApiConstants.RequestBodyParameterKey], SerializerOptions.WithConverters);
            }
            else
            {
                // Write the parameters as json in the body
                stringData = JsonConvert.SerializeObject(parameters, SerializerOptions.WithConverters);
            }
        }
        else if (format == RestRequestBodyFormat.FormData)
        {
            // Write the parameters as form data in the body
            stringData = parameters.ToFormData();
        }

        return stringData;
    }

    /// <summary>
    /// Parse an error response from the server. Only used when server returns a status other than Success(200)
    /// </summary>
    /// <param name="error">The string the request returned</param>
    protected virtual Error ParseErrorResponse(JToken error)
        => new ServerError(error.ToString());

    /// <summary>
    /// Retrieve the server time for the purpose of syncing time between client and server to prevent authentication issues
    /// </summary>
    /// <returns>Server time</returns>
    protected virtual Task<RestCallResult<DateTime>> GetServerTimestampAsync()
        => Task.FromResult(new RestCallResult<DateTime>(null, null, DateTime.UtcNow, null, null));

    protected internal virtual async Task<RestCallResult<bool>> SyncTimeAsync()
    {
        var timeSyncParams = GetTimeSyncInfo();
        if (await timeSyncParams.TimeSyncState.Semaphore.WaitAsync(0).ConfigureAwait(false))
        {
            if (!timeSyncParams.SyncTime || (DateTime.UtcNow - timeSyncParams.TimeSyncState.LastSyncTime < timeSyncParams.RecalculationInterval))
            {
                timeSyncParams.TimeSyncState.Semaphore.Release();
                return new RestCallResult<bool>(null, null, null, null, null, null, null, null, true, null);
            }

            var localTime = DateTime.UtcNow;
            var result = await GetServerTimestampAsync().ConfigureAwait(false);
            if (!result)
            {
                timeSyncParams.TimeSyncState.Semaphore.Release();
                return result.As(false);
            }

            if (TotalRequestsMade == 1)
            {
                // If this was the first request make another one to calculate the offset since the first one can be slower
                localTime = DateTime.UtcNow;
                result = await GetServerTimestampAsync().ConfigureAwait(false);
                if (!result)
                {
                    timeSyncParams.TimeSyncState.Semaphore.Release();
                    return result.As(false);
                }
            }

            // Calculate time offset between local and server
            var offset = result.Data - (localTime.AddMilliseconds(result.Response.ResponseTime!.Value.TotalMilliseconds / 2));
            timeSyncParams.UpdateTimeOffset(offset);
            timeSyncParams.TimeSyncState.Semaphore.Release();
        }

        return new RestCallResult<bool>(null, null, null, null, null, null, null, null, true, null);
    }

}
