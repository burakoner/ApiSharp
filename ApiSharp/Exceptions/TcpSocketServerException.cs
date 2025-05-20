namespace ApiSharp.Exceptions;

/// <summary>
/// Represents errors that occur during the operation of a TCP socket server.
/// </summary>
/// <remarks>This exception is typically thrown when a TCP socket server encounters an error that prevents it from
/// functioning correctly. It provides a message describing the specific issue encountered.</remarks>
/// <remarks>
/// Constructs a new instance of the <see cref="TcpSocketServerException"/> class with a specified error message.
/// </remarks>
/// <param name="message"></param>
public class TcpSocketServerException(string message) : Exception(message)
{
}
