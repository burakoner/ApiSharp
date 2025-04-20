namespace ApiSharp.Models;

/// <summary>
/// CallResult
/// </summary>
/// <param name="error"></param>
public class CallResult(Error? error)
{
    /// <summary>
    /// Error
    /// </summary>
    public Error? Error { get; internal set; } = error;

    /// <summary>
    /// Success Flag
    /// </summary>
    public bool Success => Error == null;

    /// <summary>
    /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator bool(CallResult obj)
    {
        return obj?.Success == true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}

/// <summary>
/// CallResult
/// </summary>
/// <typeparam name="T"></typeparam>
public class CallResult<T> : CallResult
{
    /// <summary>
    /// Data
    /// </summary>
    public T Data { get; internal set; }

    /// <summary>
    /// Raw Data
    /// </summary>
    public string? Raw { get; internal set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="data"></param>
    /// <param name="raw"></param>
    /// <param name="error"></param>
#pragma warning disable 8618
    protected CallResult([AllowNull] T data, string? raw, Error? error) : base(error)
    {
        Raw = raw;
#pragma warning disable 8601
        Data = data;
#pragma warning restore 8601
    }
#pragma warning restore 8618

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="data"></param>
    public CallResult(T data) : this(data, null, null) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="error"></param>
    public CallResult(Error error) : this(default, null, error) { }

    /// <summary>
    /// Create a new error result
    /// </summary>
    /// <param name="error">The error to return</param>
    /// <param name="raw">The original response data</param>
    public CallResult(Error error, string? raw) : this(default, raw, error) { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="data"></param>
    /// <param name="raw"></param>
    public CallResult(T data, string? raw) : this(data, raw, null) { }

    /// <summary>
    /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator bool(CallResult<T> obj)
    {
        // return obj?.Success == true;
        return obj != null && obj.Success && obj.Data != null;
    }

    /// <summary>
    /// GetResultOrError
    /// </summary>
    /// <param name="data"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool GetResultOrError([MaybeNullWhen(false)] out T data, [NotNullWhen(false)] out Error? error)
    {
        if (Success)
        {
            data = Data!;
            error = null;

            return true;
        }
        else
        {
            data = default;
            error = Error!;

            return false;
        }
    }

    /// <summary>
    /// Copy the WebCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="data">The data of the new type</param>
    /// <returns></returns>
    public CallResult<K> As<K>([AllowNull] K data)
    {
        return new CallResult<K>(data, Raw, Error);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public CallResult AsDataless()
    {
        return new CallResult(null);
    }

    /// <summary>
    /// Copy as a dataless result
    /// </summary>
    /// <returns></returns>
    public CallResult AsDatalessError(Error error)
    {
        return new CallResult(error);
    }

    /// <summary>
    /// Copy the WebCallResult to a new data type
    /// </summary>
    /// <typeparam name="K">The new type</typeparam>
    /// <param name="error">The error to return</param>
    /// <returns></returns>
    public CallResult<K> AsError<K>(Error error)
    {
        return new CallResult<K>(default, Raw, error);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Success ? $"Success" : $"Error: {Error}";
    }
}
