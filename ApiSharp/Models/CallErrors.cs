namespace ApiSharp.Models;

/// <summary>
/// Error
/// </summary>
/// <param name="code"></param>
/// <param name="message"></param>
/// <param name="data"></param>
public abstract class Error(int? code, string message, object data)
{
    /// <summary>
    /// Code
    /// </summary>
    public int? Code { get; set; } = code;

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Data
    /// </summary>
    public object Data { get; set; } = data;

    /// <summary>
    /// ToString Override
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Code}: {Message} {Data}";
    }
}

/// <summary>
/// Cant Connect Error
/// </summary>
public class CantConnectError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CantConnectError() : base(null, "Can't connect to the server", null) { }
}

/// <summary>
/// No Api Credentials Error
/// </summary>
public class NoApiCredentialsError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    public NoApiCredentialsError() : base(null, "No credentials provided for private endpoint", null) { }
}

/// <summary>
/// Call Error
/// </summary>
public class CallError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public CallError(string message, object data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public CallError(int code, string message, object data = null) : base(code, message, data) { }
}

/// <summary>
/// Server Error
/// </summary>
public class ServerError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public ServerError(string message, object data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public ServerError(int code, string message, object data = null) : base(code, message, data) { }
}

/// <summary>
/// Web Error
/// </summary>
public class WebError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public WebError(string message, object data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public WebError(int code, string message, object data = null) : base(code, message, data) { }
}

/// <summary>
/// Deserialize Error
/// </summary>
/// <param name="message"></param>
/// <param name="data"></param>
public class DeserializeError(string message, object data) : Error(null, message, data)
{
}

/// <summary>
/// Unknown Error
/// </summary>
/// <param name="message"></param>
/// <param name="data"></param>
public class UnknownError(string message, object data = null) : Error(null, message, data)
{
}

/// <summary>
/// Argument Error
/// </summary>
/// <param name="message"></param>
public class ArgumentError(string message) : Error(null, "Invalid parameter: " + message, null)
{
}

/// <summary>
/// Rate Limit Error
/// </summary>
/// <param name="message"></param>
public class RateLimitError(string message) : Error(null, "Rate limit exceeded: " + message, null)
{
}

/// <summary>
/// Cancellation Requested Error
/// </summary>
public class CancellationRequestedError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CancellationRequestedError() : base(null, "Cancellation requested", null) { }
}

/// <summary>
/// Invalid Operation Error
/// </summary>
/// <param name="message"></param>
public class InvalidOperationError(string message) : Error(null, message, null)
{
}
