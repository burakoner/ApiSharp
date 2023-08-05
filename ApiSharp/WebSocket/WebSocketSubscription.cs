﻿namespace ApiSharp.WebSocket;

/// <summary>
/// Socket subscription
/// </summary>
public class WebSocketSubscription
{
    /// <summary>
    /// Unique subscription id
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Exception event
    /// </summary>
    public event Action<Exception> Exception;

    /// <summary>
    /// Message handlers for this subscription. Should return true if the message is handled and should not be distributed to the other handlers
    /// </summary>
    public Action<WebSocketMessageEvent> MessageHandler { get; set; }

    /// <summary>
    /// The request object send when subscribing on the server. Either this or the `Identifier` property should be set
    /// </summary>
    public object Request { get; set; }

    /// <summary>
    /// The subscription identifier, used instead of a `Request` object to identify the subscription
    /// </summary>
    public string Identifier { get; set; }

    /// <summary>
    /// Whether this is a user subscription or an internal listener
    /// </summary>
    public bool UserSubscription { get; set; }
    
    /// <summary>
    /// If the subscription has been confirmed to be subscribed by the server
    /// </summary>
    public bool Confirmed { get; set; }

    /// <summary>
    /// Whether authentication is needed for this subscription
    /// </summary>
    public bool Authenticated { get; set; }

    /// <summary>
    /// Whether we're closing this subscription and a socket connection shouldn't be kept open for it
    /// </summary>
    public bool Closed { get; set; }

    /// <summary>
    /// Cancellation token registration, should be disposed when subscription is closed. Used for closing the subscription with 
    /// a provided cancelation token
    /// </summary>
    public CancellationTokenRegistration? CancellationTokenRegistration { get; set; }

    private WebSocketSubscription(int id, object request, string identifier, bool userSubscription, bool authenticated, Action<WebSocketMessageEvent> dataHandler)
    {
        Id = id;
        UserSubscription = userSubscription;
        MessageHandler = dataHandler;
        Request = request;
        Identifier = identifier;
        Authenticated = authenticated;
    }

    /// <summary>
    /// Create SocketSubscription for a subscribe request
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <param name="userSubscription"></param>
    /// <param name="authenticated"></param>
    /// <param name="dataHandler"></param>
    /// <returns></returns>
    public static WebSocketSubscription CreateForRequest(int id, object request, bool userSubscription,
        bool authenticated, Action<WebSocketMessageEvent> dataHandler)
    {
        return new WebSocketSubscription(id, request, null, userSubscription, authenticated, dataHandler);
    }

    /// <summary>
    /// Create SocketSubscription for an identifier
    /// </summary>
    /// <param name="id"></param>
    /// <param name="identifier"></param>
    /// <param name="userSubscription"></param>
    /// <param name="authenticated"></param>
    /// <param name="dataHandler"></param>
    /// <returns></returns>
    public static WebSocketSubscription CreateForIdentifier(int id, string identifier, bool userSubscription,
        bool authenticated, Action<WebSocketMessageEvent> dataHandler)
    {
        return new WebSocketSubscription(id, null, identifier, userSubscription, authenticated, dataHandler);
    }

    /// <summary>
    /// Invoke the exception event
    /// </summary>
    /// <param name="e"></param>
    public void InvokeExceptionHandler(Exception e)
    {
        Exception?.Invoke(e);
    }
}
