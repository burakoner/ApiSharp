namespace ApiSharp.WebSocket;

/// <summary>
/// WebSocket client
/// </summary>
public class WebSocketClient
{
    enum ProcessState
    {
        Idle,
        Processing,
        WaitingForClose,
        Reconnecting
    }

    private static int _lastWebSocketId;
    private static readonly object _webSocketIdLock = new();

    private readonly AsyncResetEvent _sendEvent;
    private readonly ConcurrentQueue<byte[]> _sendBuffer;
    private readonly SemaphoreSlim _closeSem;
    private readonly List<DateTime> _outgoingMessages;

    private ClientWebSocket _socket;
    private CancellationTokenSource _ctsSource;
    private DateTime _lastReceivedMessagesUpdate;
    private Task _processTask;
    private Task _closeTask;
    private bool _stopRequested;
    private bool _disposed;
    private ProcessState _processState;
    private DateTime _lastReconnectTime;

    /// <summary>
    /// Received messages, the size and the timstamp
    /// </summary>
    protected readonly List<WebSocketReceiveItem> _receivedMessages;

    /// <summary>
    /// Received messages lock
    /// </summary>
    protected readonly object _receivedMessagesLock;

    /// <summary>
    /// Logger for this websocket
    /// </summary>
    protected ILogger _logger;

    /// <summary>
    /// Identifier for this websocket
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Websocket Parameters
    /// </summary>
    public WebSocketParameters Parameters { get; }

    /// <summary>
    /// The timestamp this socket has been active for the last time
    /// </summary>
    public DateTime LastActionTime { get; private set; }

    /// <summary>
    /// The URI of the websocket
    /// </summary>
    public Uri Uri => Parameters.Uri;

    /// <summary>
    /// The state of the websocket
    /// </summary>
    public bool IsClosed => _socket.State == WebSocketState.Closed;

    /// <summary>
    /// The state of the websocket
    /// </summary>
    public bool IsOpen => _socket.State == WebSocketState.Open && !_ctsSource.IsCancellationRequested;

    /// <summary>
    /// The state of the websocket
    /// </summary>
    public double IncomingKbps
    {
        get
        {
            lock (_receivedMessagesLock)
            {
                UpdateReceivedMessages();

                if (!_receivedMessages.Any())
                    return 0;

                return Math.Round(_receivedMessages.Sum(v => v.Bytes) / 1000d / 3d);
            }
        }
    }

    public event Action? OnClose;
    public event Action<string>? OnMessage;
    public event Action<int>? OnRequestSent;
    public event Action<Exception>? OnError;
    public event Action? OnOpen;
    public event Action? OnReconnecting;
    public event Action? OnReconnected;
    public Func<Task<Uri>> GetReconnectionUrl { get; set; }

    /// <summary>
    /// WebSocket client constructor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="parameters"></param>
    public WebSocketClient(ILogger logger, WebSocketParameters parameters)
    {
        Id = NextWebSocketId();
        _logger = logger;

        Parameters = parameters;
        _outgoingMessages = [];
        _receivedMessages = [];
        _sendEvent = new AsyncResetEvent();
        _sendBuffer = new ConcurrentQueue<byte[]>();
        _ctsSource = new CancellationTokenSource();
        _receivedMessagesLock = new object();

        _closeSem = new SemaphoreSlim(1, 1);
        _socket = CreateWebSocket();
    }

    /// <summary>
    /// Connect to the websocket
    /// </summary>
    /// <returns></returns>
    public virtual async Task<bool> ConnectAsync()
    {
        if (!await ConnectInternalAsync().ConfigureAwait(false))
            return false;

        OnOpen?.Invoke();
        _processTask = ProcessAsync();
        return true;
    }

    private ClientWebSocket CreateWebSocket()
    {
        var cookieContainer = new CookieContainer();
        foreach (var cookie in Parameters.Cookies)
            cookieContainer.Add(new Cookie(cookie.Key, cookie.Value));

        var ws = new ClientWebSocket();
        try
        {
            ws.Options.Cookies = cookieContainer;
            foreach (var header in Parameters.Headers)
                ws.Options.SetRequestHeader(header.Key, header.Value);
            ws.Options.KeepAliveInterval = Parameters.KeepAliveInterval ?? TimeSpan.Zero;
            ws.Options.SetBuffer(65536, 65536); // Setting it to anything bigger than 65536 throws an exception in .net framework
            if (Parameters.Proxy != null)
                SetProxy(ws, Parameters.Proxy);
        }
        catch (PlatformNotSupportedException)
        {
            // Options are not supported on certain platforms (WebAssembly for instance)
            // best we can do it try to connect without setting options.
        }

        return ws;
    }

    private async Task<bool> ConnectInternalAsync()
    {
        _logger.Log(LogLevel.Debug, $"WebSocket {Id} connecting");
        try
        {
            using CancellationTokenSource tcs = new(TimeSpan.FromSeconds(10));
            await _socket.ConnectAsync(Uri, tcs.Token).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} connection failed: " + e.ToLogString());
            return false;
        }

        _logger.Log(LogLevel.Debug, $"WebSocket {Id} connected to {Uri}");
        return true;
    }

    private async Task ProcessAsync()
    {
        while (!_stopRequested)
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} starting processing tasks");
            _processState = ProcessState.Processing;
            var sendTask = SendLoopAsync();
            var receiveTask = ReceiveLoopAsync();
            var timeoutTask = Parameters.Timeout != null && Parameters.Timeout > TimeSpan.FromSeconds(0) ? CheckTimeoutAsync() : Task.CompletedTask;
            await Task.WhenAll(sendTask, receiveTask, timeoutTask).ConfigureAwait(false);
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} processing tasks finished");

            _processState = ProcessState.WaitingForClose;
            while (_closeTask == null)
                await Task.Delay(50).ConfigureAwait(false);

            await _closeTask.ConfigureAwait(false);
            _closeTask = null;

            if (!Parameters.AutoReconnect)
            {
                _processState = ProcessState.Idle;
                OnClose?.Invoke();
                return;
            }

            if (!_stopRequested)
            {
                _processState = ProcessState.Reconnecting;
                OnReconnecting?.Invoke();
            }

            var sinceLastReconnect = DateTime.UtcNow - _lastReconnectTime;
            if (sinceLastReconnect < Parameters.ReconnectInterval)
                await Task.Delay(Parameters.ReconnectInterval - sinceLastReconnect).ConfigureAwait(false);

            while (!_stopRequested)
            {
                _logger.Log(LogLevel.Debug, $"WebSocket {Id} attempting to reconnect");
                var task = GetReconnectionUrl?.Invoke();
                if (task != null)
                {
                    var reconnectUri = await task.ConfigureAwait(false);
                    if (reconnectUri != null && Parameters.Uri != reconnectUri)
                    {
                        _logger.Log(LogLevel.Debug, $"WebSocket {Id} reconnect URI set to {reconnectUri}");
                        Parameters.Uri = reconnectUri;
                    }
                }

                _socket = CreateWebSocket();
                _ctsSource.Dispose();
                _ctsSource = new CancellationTokenSource();
                while (_sendBuffer.TryDequeue(out _)) { } // Clear send buffer

                var connected = await ConnectInternalAsync().ConfigureAwait(false);
                if (!connected)
                {
                    await Task.Delay(Parameters.ReconnectInterval).ConfigureAwait(false);
                    continue;
                }

                _lastReconnectTime = DateTime.UtcNow;
                OnReconnected?.Invoke();
                break;
            }
        }

        _processState = ProcessState.Idle;
    }

    public virtual void Send(string data)
    {
        if (_ctsSource.IsCancellationRequested)
            return;

        var bytes = Parameters.Encoding.GetBytes(data);
        _logger.Log(LogLevel.Trace, $"WebSocket {Id} Adding {bytes.Length} to sent buffer");
        _sendBuffer.Enqueue(bytes);
        _sendEvent.Set();
    }

    public virtual async Task ReconnectAsync()
    {
        if (_processState != ProcessState.Processing && IsOpen)
            return;

        _logger.Log(LogLevel.Debug, $"WebSocket {Id} reconnect requested");
        _closeTask = CloseInternalAsync();
        await _closeTask.ConfigureAwait(false);
    }

    public virtual async Task CloseAsync()
    {
        await _closeSem.WaitAsync().ConfigureAwait(false);
        _stopRequested = true;

        try
        {
            if (_closeTask?.IsCompleted == false)
            {
                _logger.Log(LogLevel.Debug, $"WebSocket {Id} CloseAsync() waiting for existing close task");
                await _closeTask.ConfigureAwait(false);
                return;
            }

            if (!IsOpen)
            {
                _logger.Log(LogLevel.Debug, $"WebSocket {Id} CloseAsync() socket not open");
                return;
            }

            _logger.Log(LogLevel.Debug, $"WebSocket {Id} closing");
            _closeTask = CloseInternalAsync();
        }
        finally
        {
            _closeSem.Release();
        }

        await _closeTask.ConfigureAwait(false);
        if (_processTask != null)
            await _processTask.ConfigureAwait(false);
        OnClose?.Invoke();
        _logger.Log(LogLevel.Debug, $"WebSocket {Id} closed");
    }

    private async Task CloseInternalAsync()
    {
        if (_disposed)
            return;

        //_closeState = CloseState.Closing;
        _ctsSource.Cancel();
        _sendEvent.Set();

        if (_socket.State == WebSocketState.Open)
        {
            try
            {
                await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Can sometimes throw an exception when socket is in aborted state due to timing
                // Websocket is set to Aborted state when the cancelation token is set during SendAsync/ReceiveAsync
                // So socket might go to aborted state, might still be open
            }
        }
        else if (_socket.State == WebSocketState.CloseReceived)
        {
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", default).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Can sometimes throw an exception when socket is in aborted state due to timing
                // Websocket is set to Aborted state when the cancelation token is set during SendAsync/ReceiveAsync
                // So socket might go to aborted state, might still be open
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.Log(LogLevel.Debug, $"WebSocket {Id} disposing");
        _disposed = true;
        _socket.Dispose();
        _ctsSource.Dispose();
        _logger.Log(LogLevel.Trace, $"WebSocket {Id} disposed");
    }

    private async Task SendLoopAsync()
    {
        try
        {
            while (true)
            {
                if (_ctsSource.IsCancellationRequested)
                    break;

                await _sendEvent.WaitAsync().ConfigureAwait(false);

                if (_ctsSource.IsCancellationRequested)
                    break;

                while (_sendBuffer.TryDequeue(out var data))
                {
                    if (Parameters.RateLimitPerSecond != null)
                    {
                        // Wait for rate limit
                        DateTime? start = null;
                        while (MessagesSentLastSecond() >= Parameters.RateLimitPerSecond)
                        {
                            start ??= DateTime.UtcNow;
                            await Task.Delay(50).ConfigureAwait(false);
                        }

                        if (start != null)
                            _logger.Log(LogLevel.Debug, $"WebSocket {Id} sent delayed {Math.Round((DateTime.UtcNow - start.Value).TotalMilliseconds)}ms because of rate limit");
                    }

                    try
                    {
                        await _socket.SendAsync(new ArraySegment<byte>(data, 0, data.Length), WebSocketMessageType.Text, true, _ctsSource.Token).ConfigureAwait(false);
                        _outgoingMessages.Add(DateTime.UtcNow);
                        _logger.Log(LogLevel.Trace, $"WebSocket {Id} sent {data.Length} bytes");
                    }
                    catch (OperationCanceledException)
                    {
                        // canceled
                        break;
                    }
                    catch (Exception ioe)
                    {
                        // Connection closed unexpectedly, .NET framework
                        OnError?.Invoke(ioe);
                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();
                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Because this is running in a separate task and not awaited until the socket gets closed
            // any exception here will crash the send processing, but do so silently unless the socket get's stopped.
            // Make sure we at least let the owner know there was an error
            _logger.Log(LogLevel.Warning, $"WebSocket {Id} Send loop stopped with exception");
            OnError?.Invoke(e);
            throw;
        }
        finally
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} Send loop finished");
        }
    }

    /// <summary>
    /// Loop for receiving and reassembling data
    /// </summary>
    /// <returns></returns>
    private async Task ReceiveLoopAsync()
    {
        var buffer = new ArraySegment<byte>(new byte[65536]);
        var received = 0;
        try
        {
            while (true)
            {
                if (_ctsSource.IsCancellationRequested)
                    break;

                MemoryStream memoryStream = null;
                WebSocketReceiveResult receiveResult = null;
                bool multiPartMessage = false;
                while (true)
                {
                    try
                    {
                        receiveResult = await _socket.ReceiveAsync(buffer, _ctsSource.Token).ConfigureAwait(false);
                        received += receiveResult.Count;
                        lock (_receivedMessagesLock)
                            _receivedMessages.Add(new WebSocketReceiveItem(DateTime.UtcNow, receiveResult.Count));
                    }
                    catch (OperationCanceledException)
                    {
                        // canceled
                        break;
                    }
                    catch (Exception wse)
                    {
                        // Connection closed unexpectedly
                        OnError?.Invoke(wse);
                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();
                        break;
                    }

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        // Connection closed unexpectedly        
                        _logger.Log(LogLevel.Debug, $"WebSocket {Id} received `Close` message");
                        if (_closeTask?.IsCompleted != false)
                            _closeTask = CloseInternalAsync();
                        break;
                    }

                    if (!receiveResult.EndOfMessage)
                    {
                        // We received data, but it is not complete, write it to a memory stream for reassembling
                        multiPartMessage = true;
                        memoryStream ??= new MemoryStream();
                        _logger.Log(LogLevel.Trace, $"WebSocket {Id} received {receiveResult.Count} bytes in partial message");
                        await memoryStream.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count).ConfigureAwait(false);
                    }
                    else
                    {
                        if (!multiPartMessage)
                        {
                            // Received a complete message and it's not multi part
                            _logger.Log(LogLevel.Trace, $"WebSocket {Id} received {receiveResult.Count} bytes in single message");
                            HandleMessage(buffer.Array!, buffer.Offset, receiveResult.Count, receiveResult.MessageType);
                        }
                        else
                        {
                            // Received the end of a multipart message, write to memory stream for reassembling
                            _logger.Log(LogLevel.Trace, $"WebSocket {Id} received {receiveResult.Count} bytes in partial message");
                            await memoryStream!.WriteAsync(buffer.Array, buffer.Offset, receiveResult.Count).ConfigureAwait(false);
                        }
                        break;
                    }
                }

                lock (_receivedMessagesLock)
                    UpdateReceivedMessages();

                if (receiveResult?.MessageType == WebSocketMessageType.Close)
                {
                    // Received close message
                    break;
                }

                if (receiveResult == null || _ctsSource.IsCancellationRequested)
                {
                    // Error during receiving or cancellation requested, stop.
                    break;
                }

                if (multiPartMessage)
                {
                    // When the connection gets interupted we might not have received a full message
                    if (receiveResult?.EndOfMessage == true)
                    {
                        // Reassemble complete message from memory stream
                        _logger.Log(LogLevel.Trace, $"WebSocket {Id} reassembled message of {memoryStream!.Length} bytes");
                        HandleMessage(memoryStream!.ToArray(), 0, (int)memoryStream.Length, receiveResult.MessageType);
                        memoryStream.Dispose();
                    }
                    else
                    {
                        _logger.Log(LogLevel.Trace, $"WebSocket {Id} discarding incomplete message of {memoryStream!.Length} bytes");
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Because this is running in a separate task and not awaited until the socket gets closed
            // any exception here will crash the receive processing, but do so silently unless the socket gets stopped.
            // Make sure we at least let the owner know there was an error
            _logger.Log(LogLevel.Warning, $"WebSocket {Id} Receive loop stopped with exception");
            OnError?.Invoke(e);
            throw;
        }
        finally
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} Receive loop finished");
        }
    }

    /// <summary>
    /// Handles the message
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <param name="messageType"></param>
    private void HandleMessage(byte[] data, int offset, int count, WebSocketMessageType messageType)
    {
        string strData;
        if (messageType == WebSocketMessageType.Binary)
        {
            if (Parameters.DataInterpreterBytes == null)
                throw new Exception("Byte interpreter not set while receiving byte data");

            try
            {
                var relevantData = new byte[count];
                Array.Copy(data, offset, relevantData, 0, count);
                strData = Parameters.DataInterpreterBytes(relevantData);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"WebSocket {Id} unhandled exception during byte data interpretation: " + e.ToLogString());
                return;
            }
        }
        else
            strData = Parameters.Encoding.GetString(data, offset, count);

        if (Parameters.DataInterpreterString != null)
        {
            try
            {
                strData = Parameters.DataInterpreterString(strData);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"WebSocket {Id} unhandled exception during string data interpretation: " + e.ToLogString());
                return;
            }
        }

        try
        {
            LastActionTime = DateTime.UtcNow;
            OnMessage?.Invoke(strData);
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, $"WebSocket {Id} unhandled exception during message processing: " + e.ToLogString());
        }
    }

    /// <summary>
    /// Trigger the OnMessage event
    /// </summary>
    /// <param name="data"></param>
    protected void TriggerOnMessage(string data)
    {
        LastActionTime = DateTime.UtcNow;
        OnMessage?.Invoke(data);
    }

    /// <summary>
    /// Trigger the OnError event
    /// </summary>
    /// <param name="ex"></param>
    protected void TriggerOnError(Exception ex) => OnError?.Invoke(ex);

    /// <summary>
    /// Trigger the OnError event
    /// </summary>
    protected void TriggerOnOpen() => OnOpen?.Invoke();

    /// <summary>
    /// Trigger the OnError event
    /// </summary>
    protected void TriggerOnClose() => OnClose?.Invoke();

    /// <summary>
    /// Trigger the OnReconnecting event
    /// </summary>
    protected void TriggerOnReconnecting() => OnReconnecting?.Invoke();

    /// <summary>
    /// Trigger the OnReconnected event
    /// </summary>
    protected void TriggerOnReconnected() => OnReconnected?.Invoke();

    /// <summary>
    /// Checks if there is no data received for a period longer than the specified timeout
    /// </summary>
    /// <returns></returns>
    protected async Task CheckTimeoutAsync()
    {
        _logger.Log(LogLevel.Debug, $"WebSocket {Id} Starting task checking for no data received for {Parameters.Timeout}");
        LastActionTime = DateTime.UtcNow;
        try
        {
            while (true)
            {
                if (_ctsSource.IsCancellationRequested)
                    return;

                if (DateTime.UtcNow - LastActionTime > Parameters.Timeout)
                {
                    _logger.Log(LogLevel.Warning, $"WebSocket {Id} No data received for {Parameters.Timeout}, reconnecting socket");
                    _ = ReconnectAsync().ConfigureAwait(false);
                    return;
                }
                try
                {
                    await Task.Delay(500, _ctsSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // canceled
                    break;
                }
            }
        }
        catch (Exception e)
        {
            // Because this is running in a separate task and not awaited until the socket gets closed
            // any exception here will stop the timeout checking, but do so silently unless the socket get's stopped.
            // Make sure we at least let the owner know there was an error
            OnError?.Invoke(e);
            throw;
        }
    }

    /// <summary>
    /// Get the next identifier
    /// </summary>
    /// <returns></returns>
    private static int NextWebSocketId()
    {
        lock (_webSocketIdLock)
        {
            _lastWebSocketId++;
            return _lastWebSocketId;
        }
    }

    private int MessagesSentLastSecond()
    {
        var testTime = DateTime.UtcNow;
        _outgoingMessages.RemoveAll(r => testTime - r > TimeSpan.FromSeconds(1));
        return _outgoingMessages.Count;
    }

    /// <summary>
    /// Update the received messages list, removing messages received longer than 3s ago
    /// </summary>
    protected void UpdateReceivedMessages()
    {
        var checkTime = DateTime.UtcNow;
        if (checkTime - _lastReceivedMessagesUpdate > TimeSpan.FromSeconds(1))
        {
            foreach (var msg in _receivedMessages.ToList()) // To list here because we're removing from the list
                if (checkTime - msg.Timestamp > TimeSpan.FromSeconds(3))
                    _receivedMessages.Remove(msg);

            _lastReceivedMessagesUpdate = checkTime;
        }
    }

    /// <summary>
    /// Set proxy on socket
    /// </summary>
    /// <param name="ws"></param>
    /// <param name="proxy"></param>
    /// <exception cref="ArgumentException"></exception>
    protected virtual void SetProxy(ClientWebSocket ws, ProxyCredentials proxy)
    {
        if (!Uri.TryCreate($"{proxy.Host}:{proxy.Port}", UriKind.Absolute, out var uri))
            throw new ArgumentException("Proxy settings invalid, {proxy.Host}:{proxy.Port} not a valid URI", nameof(proxy));

        ws.Options.Proxy = uri?.Scheme == null
            ? ws.Options.Proxy = new WebProxy(proxy.Host, proxy.Port)
            : ws.Options.Proxy = new WebProxy
            {
                Address = uri
            };

        if (proxy.Username != null)
            ws.Options.Proxy.Credentials = new NetworkCredential(proxy.Username.GetString(), proxy.Password.GetString());
    }
}