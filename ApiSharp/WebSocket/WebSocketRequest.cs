namespace ApiSharp.WebSocket;

internal class WebSocketRequest
{
    public Func<JToken, bool> Handler { get; }
    public JToken Result { get; private set; }
    public bool Completed { get; private set; }
    public AsyncResetEvent Event { get; }
    public DateTime RequestTimestamp { get; set; }
    public TimeSpan Timeout { get; }

    private CancellationTokenSource cts;

    public WebSocketRequest(Func<JToken, bool> handler, TimeSpan timeout)
    {
        Handler = handler;
        Event = new AsyncResetEvent(false, false);
        RequestTimestamp = DateTime.UtcNow;
        Timeout = timeout;

        cts = new CancellationTokenSource(timeout);
        cts.Token.Register(Fail, false);
    }

    public bool CheckData(JToken data)
    {
        if (Handler(data))
        {
            Result = data;
            Completed = true;
            Event.Set();
            return true;
        }

        return false;
    }

    public void Fail()
    {
        Completed = true;
        Event.Set();
    }
}
