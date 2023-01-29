namespace ApiSharp.Models;

public abstract class CallError
{
    public int? Code { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    protected CallError(int? code, string message, object data)
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

public class CantConnectError : CallError
{
    public CantConnectError() : base(null, "Can't connect to the server", null) { }
}

public class NoApiCredentialsError : CallError
{
    public NoApiCredentialsError() : base(null, "No credentials provided for private endpoint", null) { }
}

public class ServerError : CallError
{
    public ServerError(string message, object data = null) : base(null, message, data) { }
    public ServerError(int code, string message, object data = null) : base(code, message, data) { }
}

public class WebError : CallError
{
    public WebError(string message, object data = null) : base(null, message, data) { }
    public WebError(int code, string message, object data = null) : base(code, message, data) { }
}

public class DeserializeError : CallError
{
    public DeserializeError(string message, object data) : base(null, message, data) { }
}

public class UnknownError : CallError
{
    public UnknownError(string message, object data = null) : base(null, message, data) { }
}

public class ArgumentError : CallError
{
    public ArgumentError(string message) : base(null, "Invalid parameter: " + message, null) { }
}

public class RateLimitError : CallError
{
    public RateLimitError(string message) : base(null, "Rate limit exceeded: " + message, null) { }
}

public class CancellationRequestedError : CallError
{
    public CancellationRequestedError() : base(null, "Cancellation requested", null) { }
}

public class InvalidOperationError : CallError
{
    public InvalidOperationError(string message) : base(null, message, null) { }
}
