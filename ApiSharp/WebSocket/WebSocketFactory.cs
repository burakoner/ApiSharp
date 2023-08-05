namespace ApiSharp.WebSocket;

public class WebSocketFactory
{
    public WebSocketClient CreateWebSocketClient(Log log, WebSocketParameters parameters)
    {
        return new WebSocketClient(log, parameters);
    }
}
