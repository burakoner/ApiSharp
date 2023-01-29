namespace ApiSharp.Exceptions;

public class TcpSocketClientException : Exception
{
    public TcpSocketClientException(string message):base(message) { }
}
