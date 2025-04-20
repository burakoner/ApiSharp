namespace ApiSharp.Models;

/// <summary>
/// Error
/// </summary>
/// <param name="code"></param>
/// <param name="message"></param>
/// <param name="data"></param>
public abstract class Error(int? code, string message, object? data)
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
    public object? Data { get; set; } = data;

    /// <summary>
    /// ToString Override
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Code != null ? $"[{GetType().Name}] {Code}: {Message} {Data}" : $"[{GetType().Name}] {Message} {Data}";
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected CantConnectError(int? code, string message, object? data) : base(code, message, data) { }
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

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected NoApiCredentialsError(int? code, string message, object? data) : base(code, message, data) { }
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
    public ServerError(string message, object? data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public ServerError(int code, string message, object? data = null) : base(code, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected ServerError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Web error returned by the server
/// </summary>
public class WebError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public WebError(string message, object? data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public WebError(int code, string message, object? data = null) : base(code, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected WebError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Error while deserializing data
/// </summary>
public class DeserializeError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="data">The data which caused the error</param>
    public DeserializeError(string message, object? data) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected DeserializeError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Unknown error
/// </summary>
public class UnknownError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="data">Error data</param>
    public UnknownError(string message, object? data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected UnknownError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// An invalid parameter has been provided
/// </summary>
public class ArgumentError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public ArgumentError(string message) : base(null, "Invalid parameter: " + message, null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected ArgumentError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Rate limit exceeded (client side)
/// </summary>
public abstract class BaseRateLimitError : Error
{
    /// <summary>
    /// When the request can be retried
    /// </summary>
    public DateTime? RetryAfter { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected BaseRateLimitError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Rate limit exceeded (client side)
/// </summary>
public class ClientRateLimitError : BaseRateLimitError
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public ClientRateLimitError(string message) : base(null, "Client rate limit exceeded: " + message, null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected ClientRateLimitError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Rate limit exceeded (server side)
/// </summary>
public class ServerRateLimitError : BaseRateLimitError
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public ServerRateLimitError(string message) : base(null, "Server rate limit exceeded: " + message, null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected ServerRateLimitError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Cancellation requested
/// </summary>
public class CancellationRequestedError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    public CancellationRequestedError() : base(null, "Cancellation requested", null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public CancellationRequestedError(int? code, string message, object? data) : base(code, message, data) { }
}

/// <summary>
/// Invalid operation requested
/// </summary>
public class InvalidOperationError : Error
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public InvalidOperationError(string message) : base(null, message, null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    protected InvalidOperationError(int? code, string message, object? data) : base(code, message, data) { }
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
    public CallError(string message, object? data = null) : base(null, message, data) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="data"></param>
    public CallError(int code, string message, object? data = null) : base(code, message, data) { }
}