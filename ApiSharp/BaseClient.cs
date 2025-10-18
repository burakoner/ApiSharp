namespace ApiSharp;

/// <summary>
/// Base Client
/// </summary>
/// <param name="logger"></param>
/// <param name="options"></param>
public abstract class BaseClient(ILogger? logger, BaseClientOptions options) : IDisposable
{
    /// <summary>
    /// Logger
    /// </summary>
    protected ILogger _logger = logger ?? CreateLogger();

    /// <summary>
    /// Client Options
    /// </summary>
    protected BaseClientOptions _options { get; } = options;
    private AuthenticationProvider? _authenticationProvider;
    private ApiCredentials? _credentials = options.ApiCredentials?.Copy();

    /// <summary>
    /// Disposing or Disposed
    /// </summary>
    protected bool _disposing;
    private bool _created;
    private int _id;

    /// <summary>
    /// Authentication Provider
    /// </summary>
    protected AuthenticationProvider AuthenticationProvider
    {
        get
        {
            if (!_created && !_disposing && _credentials != null)
            {
                _authenticationProvider = CreateAuthenticationProvider(_credentials);
                _created = true;
            }

            return _authenticationProvider ?? CreateAuthenticationProvider(_credentials!);
        }
    }

    /// <summary>
    /// Default Json Serializer
    /// </summary>
    private static readonly JsonSerializer _defaultSerializer = JsonSerializer.Create(SerializerOptions.WithConverters);
    //private static readonly JsonSerializer _defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings
    //{
    //    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
    //    Culture = CultureInfo.InvariantCulture,
    //});

    private static ILoggerFactory? _factory = null;

    /// <summary>
    /// Logger Factory
    /// </summary>
    public static ILoggerFactory LoggerFactory
    {
        get
        {
            _factory ??= new LoggerFactory();
            return _factory;
        }
        set { _factory = value; }
    }

    /// <summary>
    /// Creates Logger
    /// </summary>
    /// <returns></returns>
    public static ILogger CreateLogger() => LoggerFactory.CreateLogger("ApiSharp");

    /// <summary>
    /// Creates Authentication Provider
    /// </summary>
    /// <param name="credentials"></param>
    /// <returns></returns>
    protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

    /// <summary>
    /// Sets Api Credentials
    /// </summary>
    /// <param name="credentials"></param>
    public void SetApiCredentials(ApiCredentials credentials)
    {
        _created = false;
        _credentials = credentials.Copy();
        _authenticationProvider = null;
    }

    /// <summary>
    /// Incremented Id
    /// </summary>
    /// <returns></returns>
    protected int NextId()
    {
        return Interlocked.Add(ref _id, 1);
    }

    /// <summary>
    /// Validate Json
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected CallResult<JToken> ValidateJson(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            if (_options.FailOnEmptyResponse)
            {
                var info = "Empty data object received";
                _logger.Log(LogLevel.Error, info);
                return new CallResult<JToken>(new DeserializeError(info, data));
            }

            return new CallResult<JToken>(JValue.CreateNull(), data);
        }

        try
        {
            return new CallResult<JToken>(JToken.Parse(data), data);
        }
        catch (JsonReaderException jre)
        {
            var info = $"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}";
            return new CallResult<JToken>(new DeserializeError(info, data));
        }
        catch (JsonSerializationException jse)
        {
            var info = $"Deserialize JsonSerializationException: {jse.Message}";
            return new CallResult<JToken>(new DeserializeError(info, data));
        }
        catch (Exception ex)
        {
            var exceptionInfo = ex.ToLogString();
            var info = $"Deserialize Unknown Exception: {exceptionInfo}";
            return new CallResult<JToken>(new DeserializeError(info, data));
        }
    }

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="serializer"></param>
    /// <param name="requestId"></param>
    /// <returns></returns>
    protected CallResult<T> Deserialize<T>(string data, JsonSerializer? serializer = null, int? requestId = null)
    {
        var tokenResult = ValidateJson(data);
        if (!tokenResult)
        {
            _logger.Log(LogLevel.Error, tokenResult.Error!.Message);
            return new CallResult<T>(tokenResult.Error);
        }

        return Deserialize<T>(tokenResult.Data, serializer, requestId);
    }

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="serializer"></param>
    /// <param name="requestId"></param>
    /// <returns></returns>
    protected CallResult<T> Deserialize<T>(JToken obj, JsonSerializer? serializer = null, int? requestId = null)
    {
        serializer ??= _defaultSerializer;

        try
        {
            return new CallResult<T>(obj.ToObject<T>(serializer)!, obj.ToString());
        }
        catch (JsonReaderException jre)
        {
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message} Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {obj}";
            _logger.Log(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
        catch (JsonSerializationException jse)
        {
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message} data: {obj}";
            _logger.Log(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
        catch (Exception ex)
        {
            var exceptionInfo = ex.ToLogString();
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {obj}";
            _logger.Log(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
    }

    /// <summary>
    /// Deserialize
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="serializer"></param>
    /// <param name="requestId"></param>
    /// <param name="elapsedMilliseconds"></param>
    /// <returns></returns>
    protected async Task<CallResult<T>> DeserializeAsync<T>(Stream stream, JsonSerializer? serializer = null, int? requestId = null, long? elapsedMilliseconds = null)
    {
        serializer ??= _defaultSerializer;
        string? data = null;

        try
        {
            // Let the reader keep the stream open so we're able to seek if needed. The calling method will close the stream.
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);

            // If we have to output the original json data or output the data into the logging we'll have to read to full response
            // in order to log/return the json data
            if (_options.RawResponse == true)
            {
                data = await reader.ReadToEndAsync().ConfigureAwait(false);
                _logger.Log(LogLevel.Debug, $"{(requestId != null ? $"[{requestId}] " : "")}Response received{(elapsedMilliseconds != null ? $" in {elapsedMilliseconds}" : " ")}ms: " + data);
                var result = Deserialize<T>(data, serializer, requestId);
                result.Raw = data;
                return result;
            }

            // If we don't have to keep track of the original json data we can use the JsonTextReader to deserialize the stream directly
            // into the desired object, which has increased performance over first reading the string value into memory and deserializing from that
            using var jsonReader = new JsonTextReader(reader);
            _logger.Log(LogLevel.Debug, $"{(requestId != null ? $"[{requestId}] " : "")}Response received{(elapsedMilliseconds != null ? $" in {elapsedMilliseconds}" : " ")}ms");
            return new CallResult<T>(serializer.Deserialize<T>(jsonReader)!);
        }
        catch (JsonReaderException jre)
        {
            if (data == null)
            {
                if (stream.CanSeek)
                {
                    // If we can seek the stream rewind it so we can retrieve the original data that was sent
                    stream.Seek(0, SeekOrigin.Begin);
                    data = await ReadStreamAsync(stream).ConfigureAwait(false);
                }
                else
                {
                    data = "[Data only available in Trace LogLevel]";
                }
            }

            _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {data}");
            return new CallResult<T>(new DeserializeError($"Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}", data));
        }
        catch (JsonSerializationException jse)
        {
            if (data == null)
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    data = await ReadStreamAsync(stream).ConfigureAwait(false);
                }
                else
                {
                    data = "[Data only available in Trace LogLevel]";
                }
            }

            _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message}, data: {data}");
            return new CallResult<T>(new DeserializeError($"Deserialize JsonSerializationException: {jse.Message}", data));
        }
        catch (Exception ex)
        {
            if (data == null)
            {
                if (stream.CanSeek)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    data = await ReadStreamAsync(stream).ConfigureAwait(false);
                }
                else
                {
                    data = "[Data only available in Trace LogLevel]";
                }
            }

            var exceptionInfo = ex.ToLogString();
            _logger.Log(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {data}");
            return new CallResult<T>(new DeserializeError($"Deserialize Unknown Exception: {exceptionInfo}", data));
        }
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public virtual void Dispose()
    {
        _logger.Log(LogLevel.Debug, "Disposing client");
        _disposing = true;
        _credentials?.Dispose();
        _authenticationProvider?.Credentials?.Dispose();
        GC.SuppressFinalize(this);
    }
}