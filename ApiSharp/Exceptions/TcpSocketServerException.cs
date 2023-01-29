namespace ApiSharp.Exceptions;

public class TcpSocketServerException : Exception
{
    public TcpSocketServerException(string message):base(message) { }
}
