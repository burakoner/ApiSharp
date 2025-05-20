namespace ApiSharp.Enums;

/// <summary>
/// Enum to define the behavior of the API when an error occurs. The API can either return an error object or throw an exception.
/// </summary>
public enum ErrorBehavior
{
    /// <summary>
    /// Returns an error object. This is the default behavior.
    /// </summary>
    ReturnError,

    /// <summary>
    /// Throws an exception. This behavior is used when the API is configured to throw exceptions on errors.
    /// </summary>
    ThrowException,
}
