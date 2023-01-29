namespace ApiSharp.Models;

public class JsonOptions
{
    /// <summary>
    /// Modifies the default behavior of RestApiClient to swallow exceptions.
    /// When set to <code>true</code>, RestApiClient will consider the request as unsuccessful
    /// in case it fails to deserialize the response.
    /// </summary>
    public ErrorBehavior ErrorBehavior { get; set; }
}
