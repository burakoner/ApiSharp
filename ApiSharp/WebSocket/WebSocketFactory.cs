namespace ApiSharp.WebSocket;

/// <summary>
/// WebSocketFactory
/// </summary>
public class WebSocketFactory
{
    /// <summary>
    /// Create WebSocket Client
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public WebSocketClient CreateWebSocketClient(ILogger logger, WebSocketParameters parameters)
    {
        return new WebSocketClient(logger, parameters);
    }
}
