namespace ApiSharp;

public abstract class BaseClient : IDisposable
{
    protected string Name { get; }
    protected BaseClientOptions Options { get; }
    protected AuthenticationProvider _authenticationProvider;
    protected ApiCredentials _apiCredentials;
    protected bool _disposing;
    protected bool _created;
    protected int _id;
    protected Log log;

    protected AuthenticationProvider AuthenticationProvider
    {
        get
        {
            if (!_created && !_disposing && _apiCredentials != null)
            {
                _authenticationProvider = CreateAuthenticationProvider(_apiCredentials);
                _created = true;
            }

            return _authenticationProvider;
        }
    }

    private static readonly JsonSerializer _defaultSerializer = JsonSerializer.Create(new JsonSerializerSettings
    {
        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
        Culture = CultureInfo.InvariantCulture
    });

    protected BaseClient(string name, BaseClientOptions options)
    {
        Name = name;
        Options = options;
        Options.OnLoggingChanged += HandleLogConfigChange;
        _apiCredentials = options.ApiCredentials?.Copy();

        log = new Log(name);
        log.UpdateWriters(options.LogWriters);
        log.Level = options.LogLevel;
        log.Write(LogLevel.Trace, $"Client configuration: {options}, ApiSharp: v{typeof(BaseClient).Assembly.GetName().Version}, {name}: v{GetType().Assembly.GetName().Version}");
    }

    protected abstract AuthenticationProvider CreateAuthenticationProvider(ApiCredentials credentials);

    /// <summary>
    /// Handle a change in the client options log config
    /// </summary>
    private void HandleLogConfigChange()
    {
        log.UpdateWriters(Options.LogWriters);
        log.Level = Options.LogLevel;
    }

    public void SetApiCredentials(ApiCredentials credentials)
    {
        _apiCredentials = credentials?.Copy();
        _created = false;
        _authenticationProvider = null;
    }

    protected int NextId()
    {
        return Interlocked.Add(ref _id, 1);
    }

    protected CallResult<JToken> ValidateJson(string data)
    {
        if (string.IsNullOrEmpty(data))
        {
            var info = "Empty data object received";
            log.Write(LogLevel.Error, info);
            return new CallResult<JToken>(new DeserializeError(info, data));
        }

        try
        {
            return new CallResult<JToken>(JToken.Parse(data));
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

    protected CallResult<T> Deserialize<T>(string data, JsonSerializer serializer = null, int? requestId = null)
    {
        var tokenResult = ValidateJson(data);
        if (!tokenResult)
        {
                log.Write(LogLevel.Error, tokenResult.Error!.Message);
            return new CallResult<T>(tokenResult.Error);
        }

        return Deserialize<T>(tokenResult.Data, serializer, requestId);
    }

    protected CallResult<T> Deserialize<T>(JToken obj, JsonSerializer serializer = null, int? requestId = null)
    {
        serializer ??= _defaultSerializer;

        try
        {
            return new CallResult<T>(obj.ToObject<T>(serializer)!);
        }
        catch (JsonReaderException jre)
        {
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message} Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {obj}";
                log.Write(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
        catch (JsonSerializationException jse)
        {
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message} data: {obj}";
                log.Write(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
        catch (Exception ex)
        {
            var exceptionInfo = ex.ToLogString();
            var info = $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {obj}";
                log.Write(LogLevel.Error, info);
            return new CallResult<T>(new DeserializeError(info, obj));
        }
    }

    protected async Task<CallResult<T>> DeserializeAsync<T>(System.IO.Stream stream, JsonSerializer serializer = null, int? requestId = null, long? elapsedMilliseconds = null)
    {
        serializer ??= _defaultSerializer;
        string data = null;

        try
        {
            // Let the reader keep the stream open so we're able to seek if needed. The calling method will close the stream.
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);

            // If we have to output the original json data or output the data into the logging we'll have to read to full response
            // in order to log/return the json data
            if (Options.OutputOriginalData == true || log.Level == LogLevel.Trace)
            {
                data = await reader.ReadToEndAsync().ConfigureAwait(false);
                    log.Write(LogLevel.Debug, $"{(requestId != null ? $"[{requestId}] " : "")}Response received{(elapsedMilliseconds != null ? $" in {elapsedMilliseconds}" : " ")}ms{(log.Level == LogLevel.Trace ? (": " + data) : "")}");
                var result = Deserialize<T>(data, serializer, requestId);
                if (Options.OutputOriginalData == true)
                    result.OriginalData = data;
                return result;
            }

            // If we don't have to keep track of the original json data we can use the JsonTextReader to deserialize the stream directly
            // into the desired object, which has increased performance over first reading the string value into memory and deserializing from that
            using var jsonReader = new JsonTextReader(reader);
                log.Write(LogLevel.Debug, $"{(requestId != null ? $"[{requestId}] " : "")}Response received{(elapsedMilliseconds != null ? $" in {elapsedMilliseconds}" : " ")}ms");
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

                log.Write(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonReaderException: {jre.Message}, Path: {jre.Path}, LineNumber: {jre.LineNumber}, LinePosition: {jre.LinePosition}, data: {data}");
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

                log.Write(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize JsonSerializationException: {jse.Message}, data: {data}");
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
                log.Write(LogLevel.Error, $"{(requestId != null ? $"[{requestId}] " : "")}Deserialize Unknown Exception: {exceptionInfo}, data: {data}");
            return new CallResult<T>(new DeserializeError($"Deserialize Unknown Exception: {exceptionInfo}", data));
        }
    }

    private static async Task<string> ReadStreamAsync(System.IO.Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, false, 512, true);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    public virtual void Dispose()
    {
        log.Write(LogLevel.Debug, "Disposing client");
        _disposing = true;
        _apiCredentials?.Dispose();
        AuthenticationProvider?.Credentials?.Dispose();
    }
}