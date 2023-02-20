namespace ApiSharp.Models;

public class CallResult
{
    public Error Error { get; internal set; }
    public bool Success => Error == null;

    public CallResult(Error error)
    {
        Error = error;
    }

    /// <summary>
    /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator bool(CallResult obj)
    {
        return obj?.Success == true;
    }
}

public class CallResult<T> : CallResult
{
    public T Data { get; internal set; }
    public string Raw { get; internal set; }

    protected CallResult(T data, string raw, Error error) : base(error)
    {
        Raw = raw;
        Data = data;
    }

    public CallResult(T data) : this(data, null, null) { }

    public CallResult(Error error) : this(default, null, error) { }

    /// <summary>
    /// Overwrite bool check so we can use if(callResult) instead of if(callResult.Success)
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator bool(CallResult<T> obj)
    {
        return obj?.Success == true;
    }

    public bool GetResultOrError(out T data, out Error error)
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
    public CallResult<K> As<K>(K data)
    {
        return new CallResult<K>(data, Raw, Error);
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
}
