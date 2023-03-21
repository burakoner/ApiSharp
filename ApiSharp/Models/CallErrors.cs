namespace ApiSharp.Models;

public abstract class Error
{
    public int? Code { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    protected Error(int? code, string message, object data)
    {
        Code = code;
        Message = message;
        Data = data;
    }

    public override string ToString()
    {
        return $"{Code}: {Message} {Data}";
    }
}

public class CantConnectError : Error
{
    public CantConnectError() : base(null, "Can't connect to the server", null) { }
}

public class NoApiCredentialsError : Error
{
    public NoApiCredentialsError() : base(null, "No credentials provided for private endpoint", null) { }
}

public class CallError : Error
{
    public CallError(string message, object data = null) : base(null, message, data) { }
    public CallError(int code, string message, object data = null) : base(code, message, data) { }
}

public class ServerError : Error
{
    public ServerError(string message, object data = null) : base(null, message, data) { }
    public ServerError(int code, string message, object data = null) : base(code, message, data) { }
}

public class WebError : Error
{
    public WebError(string message, object data = null) : base(null, message, data) { }
    public WebError(int code, string message, object data = null) : base(code, message, data) { }
}

public class DeserializeError : Error
{
    public DeserializeError(string message, object data) : base(null, message, data) { }
}

public class UnknownError : Error
{
    public UnknownError(string message, object data = null) : base(null, message, data) { }
}

public class ArgumentError : Error
{
    public ArgumentError(string message) : base(null, "Invalid parameter: " + message, null) { }
}

public class RateLimitError : Error
{
    public RateLimitError(string message) : base(null, "Rate limit exceeded: " + message, null) { }
}

public class CancellationRequestedError : Error
{
    public CancellationRequestedError() : base(null, "Cancellation requested", null) { }
}

public class InvalidOperationError : Error
{
    public InvalidOperationError(string message) : base(null, message, null) { }
}
