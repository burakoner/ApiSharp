namespace ApiSharp.WebSocket;

public class WebSocketFactory
{
    public WebSocketClient CreateWebSocketClient(ILogger logger, WebSocketParameters parameters)
    {
        return new WebSocketClient(logger, parameters);
    }
}
