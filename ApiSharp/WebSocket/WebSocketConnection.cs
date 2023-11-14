using System.Net.Sockets;

namespace ApiSharp.WebSocket;

/// <summary>
/// A single stream connection to the server
/// </summary>
public class WebSocketConnection
{
    /// <summary>
    /// Connection lost event
    /// </summary>
    public event Action ConnectionLost;

    /// <summary>
    /// Connection closed and no reconnect is happening
    /// </summary>
    public event Action ConnectionClosed;

    /// <summary>
    /// Connecting restored event
    /// </summary>
    public event Action<TimeSpan> ConnectionRestored;

    /// <summary>
    /// The connection is paused event
    /// </summary>
    public event Action ActivityPaused;

    /// <summary>
    /// The connection is unpaused event
    /// </summary>
    public event Action ActivityUnpaused;

    /// <summary>
    /// Unhandled message event
    /// </summary>
    public event Action<JToken> UnhandledMessage;

    /// <summary>
    /// The amount of subscriptions on this connection
    /// </summary>
    public int SubscriptionCount
    {
        get
        {
            lock (_subscriptionLock)
                return _subscriptions.Count(h => h.UserSubscription);
        }
    }

    /// <summary>
    /// Get a copy of the current subscriptions
    /// </summary>
    public WebSocketSubscription[] Subscriptions
    {
        get
        {
            lock (_subscriptionLock)
                return _subscriptions.Where(h => h.UserSubscription).ToArray();
        }
    }

    /// <summary>
    /// If the connection has been authenticated
    /// </summary>
    public bool Authenticated { get; internal set; }

    /// <summary>
    /// If connection is made
    /// </summary>
    public bool Connected => _wsc.IsOpen;

    /// <summary>
    /// The unique ID of the socket
    /// </summary>
    public int Id => _wsc.Id;

    /// <summary>
    /// The current kilobytes per second of data being received, averaged over the last 3 seconds
    /// </summary>
    public double IncomingKbps => _wsc.IncomingKbps;

    /// <summary>
    /// The connection uri
    /// </summary>
    public Uri ConnectionUri => _wsc.Uri;

    /// <summary>
    /// The API client the connection is for
    /// </summary>
    public WebSocketApiClient ApiClient { get; set; }

    /// <summary>
    /// Time of disconnecting
    /// </summary>
    public DateTime? DisconnectTime { get; set; }

    /// <summary>
    /// Tag for identificaion
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// If activity is paused
    /// </summary>
    public bool PausedActivity
    {
        get => _pausedActivity;
        set
        {
            if (_pausedActivity != value)
            {
                _pausedActivity = value;
                _logger.Log(LogLevel.Information, $"WebSocket {Id} Paused activity: " + value);
                if (_pausedActivity) _ = Task.Run(() => ActivityPaused?.Invoke());
                else _ = Task.Run(() => ActivityUnpaused?.Invoke());
            }
        }
    }

    /// <summary>
    /// Status of the socket connection
    /// </summary>
    public WebSocketStatus Status
    {
        get => _status;
        private set
        {
            if (_status == value)
                return;

            var oldStatus = _status;
            _status = value;
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} status changed from {oldStatus} to {_status}");
        }
    }

    private bool _pausedActivity;
    private readonly List<WebSocketSubscription> _subscriptions;
    private readonly object _subscriptionLock = new();
    private readonly List<WebSocketRequest> _pendingRequests;
    private readonly ILogger _logger;
    private WebSocketStatus _status;

    /// <summary>
    /// The underlying websocket
    /// </summary>
    private readonly WebSocketClient _wsc;

    /// <summary>
    /// New socket connection
    /// </summary>
    /// <param name="apiClient">The api client</param>
    /// <param name="webSocketClient">The socket</param>
    /// <param name="tag"></param>
    public WebSocketConnection(ILogger logger, WebSocketApiClient apiClient, WebSocketClient webSocketClient, string tag)
    {
        _logger = logger;

        ApiClient = apiClient;
        Tag = tag;

        _pendingRequests = new List<WebSocketRequest>();
        _subscriptions = new List<WebSocketSubscription>();

        _wsc = webSocketClient;
        _wsc.OnMessage += HandleMessage;
        _wsc.OnOpen += HandleOpen;
        _wsc.OnClose += HandleClose;
        _wsc.OnReconnecting += HandleReconnecting;
        _wsc.OnReconnected += HandleReconnected;
        _wsc.OnError += HandleError;
        _wsc.GetReconnectionUrl = GetReconnectionUrlAsync;
    }

    /// <summary>
    /// Handler for a socket opening
    /// </summary>
    protected virtual void HandleOpen()
    {
        Status = WebSocketStatus.Connected;
        PausedActivity = false;
    }

    /// <summary>
    /// Handler for a socket closing without reconnect
    /// </summary>
    protected virtual void HandleClose()
    {
        Status = WebSocketStatus.Closed;
        Authenticated = false;
        lock (_subscriptionLock)
        {
            foreach (var sub in _subscriptions)
                sub.Confirmed = false;
        }
        Task.Run(() => ConnectionClosed?.Invoke());
    }

    /// <summary>
    /// Handler for a socket losing conenction and starting reconnect
    /// </summary>
    protected virtual void HandleReconnecting()
    {
        Status = WebSocketStatus.Reconnecting;
        DisconnectTime = DateTime.UtcNow;
        Authenticated = false;
        lock (_subscriptionLock)
        {
            foreach (var sub in _subscriptions)
                sub.Confirmed = false;
        }

        _ = Task.Run(() => ConnectionLost?.Invoke());
    }

    /// <summary>
    /// Get the url to connect to when reconnecting
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<Uri> GetReconnectionUrlAsync()
    {
        return await ApiClient.GetReconnectUriAsync(this).ConfigureAwait(false);
    }

    /// <summary>
    /// Handler for a socket which has reconnected
    /// </summary>
    protected virtual async void HandleReconnected()
    {
        Status = WebSocketStatus.Resubscribing;
        lock (_pendingRequests)
        {
            foreach (var pendingRequest in _pendingRequests.ToList())
            {
                pendingRequest.Fail();
                _pendingRequests.Remove(pendingRequest);
            }
        }

        if (await ProcessReconnectAsync().ConfigureAwait(false))
        {
            Status = WebSocketStatus.Connected;
            _ = Task.Run(() =>
            {
                ConnectionRestored?.Invoke(DateTime.UtcNow - DisconnectTime!.Value);
                DisconnectTime = null;
            });
        }
        else
        {
            _logger.Log(LogLevel.Warning, "Failed reconnect processing, reconnecting again");
            await _wsc.ReconnectAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handler for an error on a websocket
    /// </summary>
    /// <param name="e">The exception</param>
    protected virtual void HandleError(Exception e)
    {
        if (e is WebSocketException wse)
            _logger.Log(LogLevel.Warning, $"WebSocket {Id} error: Websocket error code {wse.WebSocketErrorCode}, details: " + e.ToLogString());
        else
            _logger.Log(LogLevel.Warning, $"WebSocket {Id} error: " + e.ToLogString());
    }

    /// <summary>
    /// Process a message received by the socket
    /// </summary>
    /// <param name="data">The received data</param>
    protected virtual void HandleMessage(string data)
    {
        var timestamp = DateTime.UtcNow;
        _logger.Log(LogLevel.Trace, $"WebSocket {Id} received data: " + data);
        if (string.IsNullOrEmpty(data)) return;

        // Check Point
        if (ApiClient.IgnoreHandlingList != null && ApiClient.IgnoreHandlingList.Contains(data))
            return;

        // To JToken
        var tokenData = data.ToJToken(_logger);
        if (tokenData == null)
        {
            data = $"\"{data}\"";
            tokenData = data.ToJToken();
            if (tokenData == null)
                return;
        }

        // Flag
        var handledResponse = false;

        // Remove any timed out requests
        WebSocketRequest[] requests;
        lock (_pendingRequests)
        {
            _pendingRequests.RemoveAll(r => r.Completed && DateTime.UtcNow - r.RequestTimestamp > TimeSpan.FromMinutes(5));
            requests = _pendingRequests.ToArray();
        }

        // Check if this message is an answer on any pending requests
        foreach (var pendingRequest in requests)
        {
            if (pendingRequest.CheckData(tokenData))
            {
                lock (_pendingRequests)
                    _pendingRequests.Remove(pendingRequest);

                if (!ApiClient.ContinueOnQueryResponse)
                    return;

                handledResponse = true;
                break;
            }
        }

        // Message was not a request response, check data handlers
        var messageEvent = new WebSocketMessageEvent(this, tokenData, ApiClient.ClientOptions.RawResponse ? data : null, timestamp);
        var (handled, userProcessTime, subscription) = HandleData(messageEvent);
        if (!handled && !handledResponse)
        {
            if (!ApiClient.UnhandledMessageExpected) _logger.Log(LogLevel.Warning, $"WebSocket {Id} Message not handled: " + tokenData);
            UnhandledMessage?.Invoke(tokenData);
        }

        var total = DateTime.UtcNow - timestamp;
        if (userProcessTime.TotalMilliseconds > 500)
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id}{(subscription == null ? "" : " subscription " + subscription!.Id)} message processing slow ({(int)total.TotalMilliseconds}ms, {(int)userProcessTime.TotalMilliseconds}ms user code), consider offloading data handling to another thread. " +
                "Data from this socket may arrive late or not at all if message processing is continuously slow.");
        }

        _logger.Log(LogLevel.Trace, $"WebSocket {Id}{(subscription == null ? "" : " subscription " + subscription!.Id)} message processed in {(int)total.TotalMilliseconds}ms, ({(int)userProcessTime.TotalMilliseconds}ms user code)");
    }

    /// <summary>
    /// Connect the websocket
    /// </summary>
    /// <returns></returns>
    public async Task<bool> ConnectAsync() => await _wsc.ConnectAsync().ConfigureAwait(false);

    /// <summary>
    /// Retrieve the underlying socket
    /// </summary>
    /// <returns></returns>
    public WebSocketClient GetWebSocketClient() => _wsc;

    /// <summary>
    /// Trigger a reconnect of the socket connection
    /// </summary>
    /// <returns></returns>
    public async Task TriggerReconnectAsync() => await _wsc.ReconnectAsync().ConfigureAwait(false);

    /// <summary>
    /// Close the connection
    /// </summary>
    /// <returns></returns>
    public async Task CloseAsync()
    {
        if (Status == WebSocketStatus.Closed || Status == WebSocketStatus.Disposed)
            return;

        if (ApiClient.WebSocketConnections.ContainsKey(Id))
            ApiClient.WebSocketConnections.TryRemove(Id, out _);

        lock (_subscriptionLock)
        {
            foreach (var subscription in _subscriptions)
            {
                if (subscription.CancellationTokenRegistration.HasValue)
                    subscription.CancellationTokenRegistration.Value.Dispose();
            }
        }

        await _wsc.CloseAsync().ConfigureAwait(false);
        _wsc.Dispose();
    }

    /// <summary>
    /// Close a subscription on this connection. If all subscriptions on this connection are closed the connection gets closed as well
    /// </summary>
    /// <param name="subscription">Subscription to close</param>
    /// <returns></returns>
    public async Task CloseAsync(WebSocketSubscription subscription)
    {
        lock (_subscriptionLock)
        {
            if (!_subscriptions.Contains(subscription))
                return;

            subscription.Closed = true;
        }

        if (Status == WebSocketStatus.Closing || Status == WebSocketStatus.Closed || Status == WebSocketStatus.Disposed)
            return;

        _logger.Log(LogLevel.Debug, $"WebSocket {Id} closing subscription {subscription.Id}");
        if (subscription.CancellationTokenRegistration.HasValue)
            subscription.CancellationTokenRegistration.Value.Dispose();

        if (subscription.Confirmed && _wsc.IsOpen)
            await ApiClient.UnsubscribeAsync(this, subscription).ConfigureAwait(false);

        bool shouldCloseConnection;
        lock (_subscriptionLock)
        {
            if (Status == WebSocketStatus.Closing)
            {
                _logger.Log(LogLevel.Debug, $"WebSocket {Id} already closing");
                return;
            }

            shouldCloseConnection = _subscriptions.All(r => !r.UserSubscription || r.Closed);
            if (shouldCloseConnection)
                Status = WebSocketStatus.Closing;
        }

        if (shouldCloseConnection)
        {
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} closing as there are no more subscriptions");
            await CloseAsync().ConfigureAwait(false);
        }

        lock (_subscriptionLock)
            _subscriptions.Remove(subscription);
    }

    /// <summary>
    /// Dispose the connection
    /// </summary>
    public void Dispose()
    {
        Status = WebSocketStatus.Disposed;
        _wsc.Dispose();
    }

    /// <summary>
    /// Add a subscription to this connection
    /// </summary>
    /// <param name="subscription"></param>
    public bool AddSubscription(WebSocketSubscription subscription)
    {
        lock (_subscriptionLock)
        {
            if (Status != WebSocketStatus.None && Status != WebSocketStatus.Connected)
                return false;

            _subscriptions.Add(subscription);
            if (subscription.UserSubscription)
                _logger.Log(LogLevel.Debug, $"WebSocket {Id} adding new subscription with id {subscription.Id}, total subscriptions on connection: {_subscriptions.Count(s => s.UserSubscription)}");
            return true;
        }
    }

    /// <summary>
    /// Get a subscription on this connection by id
    /// </summary>
    /// <param name="id"></param>
    public WebSocketSubscription GetSubscription(int id)
    {
        lock (_subscriptionLock)
            return _subscriptions.SingleOrDefault(s => s.Id == id);
    }

    /// <summary>
    /// Get a subscription on this connection by its subscribe request
    /// </summary>
    /// <param name="predicate">Filter for a request</param>
    /// <returns></returns>
    public WebSocketSubscription GetSubscriptionByRequest(Func<object, bool> predicate)
    {
        lock (_subscriptionLock)
            return _subscriptions.SingleOrDefault(s => predicate(s.Request));
    }

    /// <summary>
    /// Process data
    /// </summary>
    /// <param name="messageEvent"></param>
    /// <returns>True if the data was successfully handled</returns>
    private (bool, TimeSpan, WebSocketSubscription) HandleData(WebSocketMessageEvent messageEvent)
    {
        WebSocketSubscription currentSubscription = null;
        try
        {
            var handled = false;
            TimeSpan userCodeDuration = TimeSpan.Zero;

            // Loop the subscriptions to check if any of them signal us that the message is for them
            List<WebSocketSubscription> subscriptionsCopy;
            lock (_subscriptionLock)
                subscriptionsCopy = _subscriptions.ToList();

            foreach (var subscription in subscriptionsCopy)
            {
                currentSubscription = subscription;
                if (subscription.Request == null)
                {
                    if (ApiClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Identifier!))
                    {
                        handled = true;
                        var userSw = Stopwatch.StartNew();
                        subscription.MessageHandler(messageEvent);
                        userSw.Stop();
                        userCodeDuration = userSw.Elapsed;
                    }
                }
                else
                {
                    if (ApiClient.MessageMatchesHandler(this, messageEvent.JsonData, subscription.Request))
                    {
                        handled = true;
                        messageEvent.JsonData = ApiClient.ProcessTokenData(messageEvent.JsonData);
                        var userSw = Stopwatch.StartNew();
                        subscription.MessageHandler(messageEvent);
                        userSw.Stop();
                        userCodeDuration = userSw.Elapsed;
                    }
                }
            }

            return (handled, userCodeDuration, currentSubscription);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"WebSocket {Id} Exception during message processing\r\nException: {ex.ToLogString()}\r\nData: {messageEvent.JsonData}");
            currentSubscription?.InvokeExceptionHandler(ex);
            return (false, TimeSpan.Zero, null);
        }
    }

    /// <summary>
    /// Send data and wait for an answer
    /// </summary>
    /// <typeparam name="T">The data type expected in response</typeparam>
    /// <param name="obj">The object to send</param>
    /// <param name="timeout">The timeout for response</param>
    /// <param name="handler">The response handler, should return true if the received JToken was the response to the request</param>
    /// <returns></returns>
    public virtual async Task SendAndWaitAsync<T>(T obj, TimeSpan timeout, Func<JToken, bool> handler)
    {
        var pending = new WebSocketRequest(handler, timeout);
        lock (_pendingRequests)
        {
            _pendingRequests.Add(pending);
        }

        var sendOk = Send(obj);
        if (!sendOk)
        {
            pending.Fail();
            return;
        }

        while (true)
        {
            if (!_wsc.IsOpen)
            {
                pending.Fail();
                return;
            }

            if (pending.Completed)
                return;

            await pending.Event.WaitAsync(TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);

            if (pending.Completed)
                return;
        }
    }

    /// <summary>
    /// Send data over the websocket connection
    /// </summary>
    /// <typeparam name="T">The type of the object to send</typeparam>
    /// <param name="obj">The object to send</param>
    /// <param name="nullValueHandling">How null values should be serialized</param>
    public virtual bool Send<T>(T obj, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
    {
        if (obj is string str)
            return Send(str);
        else
            return Send(JsonConvert.SerializeObject(obj, Formatting.None, new JsonSerializerSettings { NullValueHandling = nullValueHandling }));
    }

    /// <summary>
    /// Send string data over the websocket connection
    /// </summary>
    /// <param name="data">The data to send</param>
    public virtual bool Send(string data)
    {
        _logger.Log(LogLevel.Trace, $"WebSocket {Id} sending data: {data}");
        try
        {
            _wsc.Send(data);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<CallResult<bool>> ProcessReconnectAsync()
    {
        if (!_wsc.IsOpen)
            return new CallResult<bool>(new WebError("WebSocket is not connected"));

        bool anySubscriptions = false;
        lock (_subscriptionLock)
            anySubscriptions = _subscriptions.Any(s => s.UserSubscription);

        if (!anySubscriptions)
        {
            // No need to resubscribe anything
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} Nothing to resubscribe, closing connection");
            _ = _wsc.CloseAsync();
            return new CallResult<bool>(true);
        }

        bool anyAuthenticated = false;
        lock (_subscriptionLock)
            anyAuthenticated = _subscriptions.Any(s => s.Authenticated);

        if (anyAuthenticated)
        {
            // If we reconnected a authenticated connection we need to re-authenticate
            var authResult = await ApiClient.AuthenticateAsync(this).ConfigureAwait(false);
            if (!authResult)
            {
                _logger.Log(LogLevel.Warning, $"WebSocket {Id} authentication failed on reconnected socket. Disconnecting and reconnecting.");
                return authResult;
            }

            Authenticated = true;
            _logger.Log(LogLevel.Debug, $"WebSocket {Id} authentication succeeded on reconnected socket.");
        }

        // Get a list of all subscriptions on the socket
        var subscriptionList = new List<WebSocketSubscription>();
        lock (_subscriptionLock)
        {
            foreach (var subscription in _subscriptions)
            {
                if (subscription.Request != null)
                    subscriptionList.Add(subscription);
                else
                    subscription.Confirmed = true;
            }
        }

        foreach (var subscription in subscriptionList.Where(s => s.Request != null))
        {
            var result = await ApiClient.RevitalizeRequestAsync(subscription.Request!).ConfigureAwait(false);
            if (!result)
            {
                _logger.Log(LogLevel.Warning, "Failed request revitalization: " + result.Error);
                return result.As<bool>(false);
            }
        }

        // Foreach subscription which is subscribed by a subscription request we will need to resend that request to resubscribe
        for (var i = 0; i < subscriptionList.Count; i += ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerConnection)
        {
            if (!_wsc.IsOpen)
                return new CallResult<bool>(new WebError("WebSocket is not connected"));

            var taskList = new List<Task<CallResult<bool>>>();
            foreach (var subscription in subscriptionList.Skip(i).Take(ApiClient.ClientOptions.MaxConcurrentResubscriptionsPerConnection))
                taskList.Add(ApiClient.SubscribeAndWaitAsync(this, subscription.Request!, subscription));

            await Task.WhenAll(taskList).ConfigureAwait(false);
            if (taskList.Any(t => !t.Result.Success))
                return taskList.First(t => !t.Result.Success).Result;
        }

        foreach (var subscription in subscriptionList)
            subscription.Confirmed = true;

        if (!_wsc.IsOpen)
            return new CallResult<bool>(new WebError("WebSocket is not connected"));

        _logger.Log(LogLevel.Debug, $"WebSocket {Id} all subscription successfully resubscribed on reconnected socket.");
        return new CallResult<bool>(true);
    }

    internal async Task UnsubscribeAsync(WebSocketSubscription subscription)
    {
        await ApiClient.UnsubscribeAsync(this, subscription).ConfigureAwait(false);
    }

    internal async Task<CallResult<bool>> ResubscribeAsync(WebSocketSubscription subscription)
    {
        if (!_wsc.IsOpen)
            return new CallResult<bool>(new UnknownError("WebSocket is not connected"));

        return await ApiClient.SubscribeAndWaitAsync(this, subscription.Request!, subscription).ConfigureAwait(false);
    }
}
