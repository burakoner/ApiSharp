namespace ApiSharp.WebSocket;

internal class WebSocketRequest : IDisposable
{
    public Func<JToken, bool> Handler { get; }
    public JToken? Result { get; private set; }
    public bool Completed { get; private set; }
    public AsyncResetEvent Event { get; }
    public DateTime RequestTimestamp { get; set; }
    public TimeSpan Timeout { get; }

    private readonly CancellationTokenSource cts;
    private readonly CancellationTokenRegistration timeoutRegistration;
    private int disposeState;

    public WebSocketRequest(Func<JToken, bool> handler, TimeSpan timeout)
    {
        Handler = handler;
        Event = new AsyncResetEvent(false, false);
        RequestTimestamp = DateTime.UtcNow;
        Timeout = timeout;

        cts = new CancellationTokenSource(timeout);
        timeoutRegistration = cts.Token.Register(Fail, false);
    }

    public bool CheckData(JToken data)
    {
        if (!Completed && Handler(data))
        {
            Result = data;
            Complete();
            return true;
        }

        return false;
    }

    public void Fail()
    {
        if (Completed)
            return;

        Complete();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposeState, 1) == 1)
            return;

        timeoutRegistration.Dispose();
        cts.Dispose();
    }

    private void Complete()
    {
        Completed = true;
        Event.Set();
        Dispose();
    }
}
