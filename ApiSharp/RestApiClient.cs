namespace ApiSharp;

public abstract class RestApiClient : BaseClient
{
    /// <summary>
    /// The factory for creating requests. Used for unit testing
    /// </summary>
    protected IRequestFactory RequestFactory { get; set; } = new RequestFactory();

    /// <summary>
    /// Request headers to be sent with each request
    /// </summary>
    protected Dictionary<string, string> StandardRequestHeaders { get; set; }

    /// <summary>
    /// Total amount of requests made with this API client
    /// </summary>
    protected int TotalRequestsMade { get; set; }

    /// <summary>
    /// Where to put the parameters for requests with different Http methods
    /// </summary>
    protected Dictionary<HttpMethod, HttpMethodParameterPosition> ParameterPositions { get; set; } = new()
    {
        { HttpMethod.Get, HttpMethodParameterPosition.InUri },
        { HttpMethod.Post, HttpMethodParameterPosition.InBody },
        { HttpMethod.Delete, HttpMethodParameterPosition.InBody },
        { HttpMethod.Put, HttpMethodParameterPosition.InBody }
    };

    /// <summary>
    /// Request body content type
    /// </summary>
    protected RequestBodyFormat RequestBodyFormat = RequestBodyFormat.Json;

    /// <summary>
    /// Whether or not we need to manually parse an error instead of relying on the http status code
    /// </summary>
    protected bool ManualParseError = false;

    /// <summary>
    /// How to serialize array parameters when making requests
    /// </summary>
    protected ArraySerialization ArraySerialization = ArraySerialization.Array;

    /// <summary>
    /// What request body should be set when no data is send (only used in combination with postParametersPosition.InBody)
    /// </summary>
    protected string RequestBodyEmptyContent = "{}";

    public new RestApiClientOptions Options { get { return (RestApiClientOptions)base.Options; } }

    protected RestApiClient() : this("", new())
    {
    }

    protected RestApiClient(string name, RestApiClientOptions options) : base(name, options ?? new())
    {
        RequestFactory.Configure(options.HttpOptions.RequestTimeout, options.Proxy, options.HttpClient);
    }

    /// <summary>
    /// Get time sync info for an API client
    /// </summary>
    /// <returns></returns>
    protected abstract TimeSyncInfo GetTimeSyncInfo();

    /// <summary>
    /// Get time offset for an API client
    /// </summary>
    /// <returns></returns>
    public abstract TimeSpan GetTimeOffset();

    protected virtual async Task<RestCallResult<T>> SendRequestAsync<T>(
    Uri uri,
    HttpMethod method,
    CancellationToken cancellationToken,
    Dictionary<string, object> parameters = null,
    bool signed = false,
    HttpMethodParameterPosition? parameterPosition = null,
    ArraySerialization? arraySerialization = null,
    int requestWeight = 1,
    JsonSerializer deserializer = null,
    Dictionary<string, string> additionalHeaders = null,
    bool ignoreRatelimit = false
    ) where T : class
    {
        var request = await PrepareRequestAsync(uri, method, cancellationToken, parameters, signed, parameterPosition, arraySerialization, requestWeight, deserializer, additionalHeaders, ignoreRatelimit).ConfigureAwait(false);
        if (!request)
            return new RestCallResult<T>(request.Error!);

        return await GetResponseAsync<T>(request.Data, deserializer, cancellationToken, false).ConfigureAwait(false);
    }

    /// <summary>
    /// Prepares a request to be sent to the server
    /// </summary>
    /// <param name="uri">The uri to send the request to</param>
    /// <param name="method">The method of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="parameters">The parameters of the request</param>
    /// <param name="signed">Whether or not the request should be authenticated</param>
    /// <param name="parameterPosition">Where the parameters should be placed, overwrites the value set in the client</param>
    /// <param name="arraySerialization">How array parameters should be serialized, overwrites the value set in the client</param>
    /// <param name="requestWeight">Credits used for the request</param>
    /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
    /// <param name="additionalHeaders">Additional headers to send with the request</param>
    /// <param name="ignoreRatelimit">Ignore rate limits for this request</param>
    /// <returns></returns>
    protected virtual async Task<CallResult<IRequest>> PrepareRequestAsync(
        Uri uri,
        HttpMethod method,
        CancellationToken cancellationToken,
        Dictionary<string, object> parameters = null,
        bool signed = false,
        HttpMethodParameterPosition? parameterPosition = null,
        ArraySerialization? arraySerialization = null,
        int requestWeight = 1,
        JsonSerializer deserializer = null,
        Dictionary<string, string> additionalHeaders = null,
        bool ignoreRatelimit = false)
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
                    log.Write(LogLevel.Debug, $"[{requestId}] Failed to sync time, aborting request: " + syncTimeResult.Error);
                    return syncTimeResult.As<IRequest>(default);
                }
            }
        }

        if (!ignoreRatelimit)
        {
            foreach (var limiter in Options.RateLimiters)
            {
                var limitResult = await limiter.LimitRequestAsync(log, uri.AbsolutePath, method, signed, Options.ApiCredentials?.Key, Options.RateLimitingBehavior, requestWeight, cancellationToken).ConfigureAwait(false);
                if (!limitResult.Success)
                    return new CallResult<IRequest>(limitResult.Error!);
            }
        }

        if (signed && AuthenticationProvider == null)
        {
            log.Write(LogLevel.Warning, $"[{requestId}] Request {uri.AbsolutePath} failed because no ApiCredentials were provided");
            return new CallResult<IRequest>(new NoApiCredentialsError());
        }

        log.Write(LogLevel.Information, $"[{requestId}] Creating request for " + uri);
        var paramsPosition = parameterPosition ?? ParameterPositions[method];
        var request = ConstructRequest(uri, method, parameters?.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value), signed, paramsPosition, arraySerialization ?? this.ArraySerialization, requestId, additionalHeaders);

        string paramString = "";
        if (paramsPosition == HttpMethodParameterPosition.InBody)
            paramString = $" with request body '{request.Content}'";

        var headers = request.GetHeaders();
        if (headers.Any())
            paramString += " with headers " + string.Join(", ", headers.Select(h => h.Key + $"=[{string.Join(",", h.Value)}]"));

        TotalRequestsMade++;
        log.Write(LogLevel.Trace, $"[{requestId}] Sending {method}{(signed ? " signed" : "")} request to {request.Uri}{paramString ?? " "}{(Options.Proxy == null ? "" : $" via proxy {Options.Proxy.Host}")}");
        return new CallResult<IRequest>(request);
    }

    /// <summary>
    /// Executes the request and returns the result deserialized into the type parameter class
    /// </summary>
    /// <param name="request">The request object to execute</param>
    /// <param name="deserializer">The JsonSerializer to use for deserialization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="expectedEmptyResponse">If an empty response is expected</param>
    /// <returns></returns>
    protected virtual async Task<RestCallResult<T>> GetResponseAsync<T>(
        IRequest request,
        JsonSerializer deserializer,
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
                    log.Write(LogLevel.Debug, $"[{request.RequestId}] Response received in {sw.ElapsedMilliseconds}ms{(log.Level == LogLevel.Trace ? (": " + data) : "")}");

                    if (!expectedEmptyResponse)
                    {
                        // Validate if it is valid json. Sometimes other data will be returned, 502 error html pages for example
                        var parseResult = ValidateJson(data);
                        if (!parseResult.Success)
                            return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, parseResult.Error!);

                        // Let the library implementation see if it is an error response, and if so parse the error
                        var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                        if (error != null)
                            return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error!);

                        // Not an error, so continue deserializing
                        var deserializeResult = Deserialize<T>(parseResult.Data, deserializer, request.RequestId);
                        return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), deserializeResult.Data, deserializeResult.Error);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(data))
                        {
                            var parseResult = ValidateJson(data);
                            if (!parseResult.Success)
                                // Not empty, and not json
                                return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, parseResult.Error!);

                            var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);
                            if (error != null)
                                // Error response
                                return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, error!);
                        }

                        // Empty success response; okay
                        return new RestCallResult<T>(response.StatusCode, response.ResponseHeaders, sw.Elapsed, Options.OutputOriginalData ? data : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, default);
                    }
                }
                else
                {
                    if (expectedEmptyResponse)
                    {
                        // We expected an empty response and the request is successful and don't manually parse errors, so assume it's correct
                        responseStream.Close();
                        response.Close();

                        return new RestCallResult<T>(statusCode, headers, sw.Elapsed, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, null);
                    }

                    // Success status code, and we don't have to check for errors. Continue deserializing directly from the stream
                    var desResult = await DeserializeAsync<T>(responseStream, deserializer, request.RequestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
                    responseStream.Close();
                    response.Close();

                    return new RestCallResult<T>(statusCode, headers, sw.Elapsed, Options.OutputOriginalData ? desResult.OriginalData : null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), desResult.Data, desResult.Error);
                }
            }
            else
            {
                // Http status code indicates error
                using var reader = new StreamReader(responseStream);
                var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                log.Write(LogLevel.Warning, $"[{request.RequestId}] Error received in {sw.ElapsedMilliseconds}ms: {data}");
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
            log.Write(LogLevel.Warning, $"[{request.RequestId}] Request exception: " + exceptionInfo);
            return new RestCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new WebError(exceptionInfo));
        }
        catch (OperationCanceledException canceledException)
        {
            if (cancellationToken != default && canceledException.CancellationToken == cancellationToken)
            {
                // Cancellation token canceled by caller
                log.Write(LogLevel.Warning, $"[{request.RequestId}] Request canceled by cancellation token");
                return new RestCallResult<T>(null, null, null, null, request.Uri.ToString(), request.Content, request.Method, request.GetHeaders(), default, new CancellationRequestedError());
            }
            else
            {
                // Request timed out
                log.Write(LogLevel.Warning, $"[{request.RequestId}] Request timed out: " + canceledException.ToLogString());
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
    /// <param name="parameters">The parameters of the request</param>
    /// <param name="signed">Whether or not the request should be authenticated</param>
    /// <param name="parameterPosition">Where the parameters should be placed</param>
    /// <param name="arraySerialization">How array parameters should be serialized</param>
    /// <param name="requestId">Unique id of a request</param>
    /// <param name="additionalHeaders">Additional headers to send with the request</param>
    /// <returns></returns>
    protected virtual IRequest ConstructRequest(
        Uri uri,
        HttpMethod method,
        Dictionary<string, object> parameters,
        bool signed,
        HttpMethodParameterPosition parameterPosition,
        ArraySerialization arraySerialization,
        int requestId,
        Dictionary<string, string> additionalHeaders)
    {
        parameters ??= new Dictionary<string, object>();

        for (var i = 0; i < parameters.Count; i++)
        {
            var kvp = parameters.ElementAt(i);
            if (kvp.Value is Func<object> delegateValue)
                parameters[kvp.Key] = delegateValue();
        }

        if (parameterPosition == HttpMethodParameterPosition.InUri)
        {
            foreach (var parameter in parameters)
                uri = uri.AddQueryParmeter(parameter.Key, parameter.Value.ToString());
        }

        var headers = new Dictionary<string, string>();
        var uriParameters = parameterPosition == HttpMethodParameterPosition.InUri ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
        var bodyParameters = parameterPosition == HttpMethodParameterPosition.InBody ? new SortedDictionary<string, object>(parameters) : new SortedDictionary<string, object>();
        AuthenticationProvider?.AuthenticateRestApi(this, uri, method, parameters, signed, arraySerialization, parameterPosition, out uriParameters, out bodyParameters, out headers);

        // Sanity check
        foreach (var param in parameters)
        {
            if (!uriParameters.ContainsKey(param.Key) && !bodyParameters.ContainsKey(param.Key))
            {
                throw new Exception($"Missing parameter {param.Key} after authentication processing. AuthenticationProvider implementation " +
                    $"should return provided parameters in either the uri or body parameters output");
            }
        }

        // Add the auth parameters to the uri, start with a new URI to be able to sort the parameters including the auth parameters            
        uri = uri.SetParameters(uriParameters, arraySerialization);

        var request = RequestFactory.Create(method, uri, requestId);
        request.Accept = RestApiConstants.JSON_CONTENT_HEADER;

        foreach (var header in headers)
            request.AddHeader(header.Key, header.Value);

        if (additionalHeaders != null)
        {
            foreach (var header in additionalHeaders)
                request.AddHeader(header.Key, header.Value);
        }

        if (StandardRequestHeaders != null)
        {
            foreach (var header in StandardRequestHeaders)
            {
                // Only add it if it isn't overwritten
                if (additionalHeaders?.ContainsKey(header.Key) != true)
                    request.AddHeader(header.Key, header.Value);
            }
        }

        if (parameterPosition == HttpMethodParameterPosition.InBody)
        {
            var contentType = RequestBodyFormat == RequestBodyFormat.Json ? RestApiConstants.JSON_CONTENT_HEADER : RestApiConstants.FORM_CONTENT_HEADER;
            if (bodyParameters.Any())
                WriteParamBody(request, bodyParameters, contentType);
            else
                request.SetContent(RequestBodyEmptyContent, contentType);
        }

        return request;
    }

    /// <summary>
    /// Writes the parameters of the request to the request object body
    /// </summary>
    /// <param name="request">The request to set the parameters on</param>
    /// <param name="parameters">The parameters to set</param>
    /// <param name="contentType">The content type of the data</param>
    protected virtual void WriteParamBody(IRequest request, SortedDictionary<string, object> parameters, string contentType)
    {
        if (RequestBodyFormat == RequestBodyFormat.Json)
        {
            // Write the parameters as json in the body
            var stringData = JsonConvert.SerializeObject(parameters);
            request.SetContent(stringData, contentType);
        }
        else if (RequestBodyFormat == RequestBodyFormat.FormData)
        {
            // Write the parameters as form data in the body
            var stringData = parameters.ToFormData();
            request.SetContent(stringData, contentType);
        }
    }

    /// <summary>
    /// Parse an error response from the server. Only used when server returns a status other than Success(200)
    /// </summary>
    /// <param name="error">The string the request returned</param>
    /// <returns></returns>
    protected virtual CallError ParseErrorResponse(JToken error)
    {
        return new ServerError(error.ToString());
    }

    /// <summary>
    /// Retrieve the server time for the purpose of syncing time between client and server to prevent authentication issues
    /// </summary>
    /// <returns>Server time</returns>
    protected abstract Task<RestCallResult<DateTime>> GetServerTimestampAsync();

    internal async Task<RestCallResult<bool>> SyncTimeAsync()
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
            var offset = result.Data - (localTime.AddMilliseconds(result.ResponseTime!.Value.TotalMilliseconds / 2));
            timeSyncParams.UpdateTimeOffset(offset);
            timeSyncParams.TimeSyncState.Semaphore.Release();
        }

        return new RestCallResult<bool>(null, null, null, null, null, null, null, null, true, null);
    }


































    /*
    protected async Task<RestCallResult<T>> RequestAsync<T>(
        Uri uri,
        HttpMethod method,
        Dictionary<string, string> headers = null,
        List<KeyValuePair<string, string>> query = null,
        object body = null,
        bool signed = true,
        int requestWeight = 1,
        JsonSerializer deserializer = null,
        CancellationToken ct = default)
    {
        // Request
        var req = ConstructRequest(uri, method, headers, body, signed);
        var request = req.Request;
        var requestId = req.RequestId;

        try
        {
            var sw = Stopwatch.StartNew();
            var call = await ExecuteAsync(uri, req, signed, requestWeight, ct);
            sw.Stop();

            if (!call.Success)
            {
                return new RestCallResult<T>(call.Error);
            }

            // Shortcuts
            var response = call.Data.Response;
            var responseStream = await call.Data.Response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Check Point
            if (!response.IsSuccessStatusCode)
            {
                // Http status code indicates error
                using var reader = new StreamReader(responseStream);
                var data = await reader.ReadToEndAsync().ConfigureAwait(false);
                log.Write(LogLevel.Warning, $"[{requestId}] Error received in {sw.ElapsedMilliseconds}ms: {data}");
                responseStream.Close();
                response.Dispose();

                // Json Validation
                var parseResult = ValidateJson(data);
                var error = parseResult.Success ? ParseErrorResponse(parseResult.Data) : new ServerError(data)!;
                if (error.Code == null || error.Code == 0)
                    error.Code = (int)response.StatusCode;

                // Error Response
                return new RestCallResult<T>(
                    response.StatusCode,
                    response.Headers,
                    sw.Elapsed,
                    Options.OutputOriginalData ? data : null,
                    request.RequestUri.ToString(),
                    request.Content?.ToString(),
                    request.Method,
                    request.Headers,
                    default,
                    error);
            }
            else
            {
                // Read Response
                var data = await response.Content.ReadAsStringAsync();
                log.Write(LogLevel.Debug, $"[{requestId}] Response received in {sw.ElapsedMilliseconds}ms{(log.Level == LogLevel.Trace ? (": " + data) : "")}");

                // Empty Success Response; Okay
                if (string.IsNullOrEmpty(data))
                {
                    return new RestCallResult<T>(
                        response.StatusCode,
                        response.Headers,
                        sw.Elapsed,
                        Options.OutputOriginalData ? data : null,
                        request.RequestUri.ToString(),
                        request.Content?.ToString(),
                        request.Method,
                        request.Headers,
                        default,
                        default);
                }

                // Json Validation
                var parseResult = ValidateJson(data);

                // Not Empty, and Not Json
                if (!parseResult.Success)
                {
                    return new RestCallResult<T>(
                        response.StatusCode,
                        response.Headers,
                        sw.Elapsed,
                        Options.OutputOriginalData ? data : null,
                        request.RequestUri.ToString(),
                        request.Content?.ToString(),
                        request.Method,
                        request.Headers,
                        default,
                        parseResult.Error!);
                }

                // Parse Error
                var error = await TryParseErrorAsync(parseResult.Data).ConfigureAwait(false);

                // Error Response
                if (error != null)
                {
                    return new RestCallResult<T>(
                        response.StatusCode,
                        response.Headers,
                        sw.Elapsed,
                        Options.OutputOriginalData ? data : null,
                        request.RequestUri.ToString(),
                        request.Content?.ToString(),
                        request.Method,
                        request.Headers,
                        default,
                        error!);
                }
            }

            // Success status code, and we don't have to check for errors.
            // Continue deserializing directly from the stream
            var desResult = await DeserializeAsync<T>(responseStream, deserializer, requestId, sw.ElapsedMilliseconds).ConfigureAwait(false);
            responseStream.Close();
            response.Dispose();

            // Return
            return new RestCallResult<T>(
                response.StatusCode,
                response.Headers,
                sw.Elapsed,
                Options.OutputOriginalData ? desResult.OriginalData : null,
                request.RequestUri.ToString(),
                request.Content?.ToString(),
                request.Method,
                request.Headers,
                desResult.Data,
                desResult.Error);
        }
        catch (HttpRequestException requestException)
        {
            // Request exception, can't reach server for instance
            var exceptionInfo = requestException.ToLogString();
            log.Write(LogLevel.Warning, $"[{requestId}] Request exception: " + exceptionInfo);
            return new RestCallResult<T>(null, null, null, null, request.RequestUri.ToString(), request.Content?.ToString(), request.Method, request.Headers, default, new WebError(exceptionInfo));
        }
        catch (OperationCanceledException canceledException)
        {
            if (ct != default && canceledException.CancellationToken == ct)
            {
                // Cancellation token canceled by caller
                log.Write(LogLevel.Warning, $"[{requestId}] Request canceled by cancellation token");
                return new RestCallResult<T>(null, null, null, null, request.RequestUri.ToString(), request.Content?.ToString(), request.Method, request.Headers, default, new CancellationRequestedError());
            }
            else
            {
                // Request timed out
                log.Write(LogLevel.Warning, $"[{requestId}] Request timed out: " + canceledException.ToLogString());
                return new RestCallResult<T>(null, null, null, null, request.RequestUri.ToString(), request.Content?.ToString(), request.Method, request.Headers, default, new WebError($"[{requestId}] Request timed out"));
            }
        }
    }

    private async Task<CallResult<RestResponse>> ExecuteAsync(
        Uri uri,
        RestRequest request,
        bool signed = false, int requestWeight = 1, CancellationToken ct = default)
    {
        // Rate Limiters
        if (!Options.IgnoreRateLimiters)
        {
            foreach (var limiter in Options.RateLimiters)
            {
                var limitResult = await limiter.LimitRequestAsync(log, uri.AbsolutePath, request.Request.Method, signed, Options.ApiCredentials?.Key, Options.RateLimitingBehavior, requestWeight, ct).ConfigureAwait(false);
                if (!limitResult.Success)
                    return new CallResult<RestResponse>(limitResult.Error!);
            }
        }

        // Proxy
        var handler = new HttpClientHandler()
        {
            Proxy = this.Options.Proxy == null ? null : new WebProxy
            {
                Address = new Uri($"{this.Options.Proxy.Host}:{this.Options.Proxy.Port}"),
                Credentials = this.Options.Proxy.Password == null ? null
                : new NetworkCredential(this.Options.Proxy.Username.GetString(), this.Options.Proxy.Password.GetString())
            }
        };

        // Http Client
        using var client = new HttpClient(handler);
        client.BaseAddress = new Uri(this.Options.BaseAddress);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Options.HttpOptions.AcceptMimeType));
        client.DefaultRequestHeaders.Add("User-Agent", Options.HttpOptions.UserAgent);
        client.Timeout = Options.HttpOptions.Timeout;

        // Send
        var response = await client.SendAsync(request.Request, ct);

        // Return
        return new CallResult<RestResponse>(new RestResponse
        {
            Request = request,
            Response = response
        });
    }

    private RestRequest ConstructRequest(
        Uri uri,
        HttpMethod method,
        Dictionary<string, string> headers,
        // List<KeyValuePair<string, string>> query,
        object body,
        bool signed)
    {
        // Check Point
        headers ??= new Dictionary<string, string>();
        // query ??= new List<KeyValuePair<string, string>>();

        // Authentication
        this.Options.AuthenticationProvider?.AuthenticateRestApi(uri, method, headers, body, signed, Options);

        // Headers
        var request = new HttpRequestMessage(method, uri);
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        // Method
        request.Method = method;

        // Body
        if (body != null)
        {
            if (Options.HttpOptions.BodyFormat == BodyFormat.Json)
            {
                if (body is string bodyString) request.Content = new StringContent(bodyString, Options.Encoding, RestApiConstants.JSON_CONTENT_HEADER);
                else request.Content = new StringContent(JsonConvert.SerializeObject(body), Options.Encoding, RestApiConstants.JSON_CONTENT_HEADER);
            }

            else if (Options.HttpOptions.BodyFormat == BodyFormat.Text)
            {
                if (body is string bodyString) request.Content = new StringContent(bodyString, Options.Encoding, RestApiConstants.TEXT_CONTENT_HEADER);
                else request.Content = new StringContent(JsonConvert.SerializeObject(body), Options.Encoding, RestApiConstants.TEXT_CONTENT_HEADER);
            }

            else if (Options.HttpOptions.BodyFormat == BodyFormat.FormUrlEncoded)
            {
                var kvplist = body.GetType().GetProperties()
                    .ToDictionary(p => p.Name, p => p.GetValue(body).ToString())
                    .ToList();

                request.Content = new FormUrlEncodedContent(kvplist);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(RestApiConstants.FORM_CONTENT_HEADER);
            }
        }

        // Return
        return new RestRequest
        {
            RequestId = NextId(),
            Request = request,
        };
    }
    */
}