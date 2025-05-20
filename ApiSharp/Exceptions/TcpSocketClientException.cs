namespace ApiSharp.Exceptions;

/// <summary>
/// Represents errors that occur during TCP socket client operations.
/// </summary>
/// <remarks>This exception is typically thrown when a TCP socket client encounters an error that prevents it from
/// completing an operation, such as connection failures or data transmission issues. Use the <see
/// cref="Exception.Message"/> property to retrieve details about the specific error.</remarks>
/// <remarks>
/// Constructs a new instance of the <see cref="TcpSocketClientException"/> class with a specified error message.
/// </remarks>
/// <param name="message"></param>
public class TcpSocketClientException(string message) : Exception(message)
{
}
