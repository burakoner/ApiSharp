namespace ApiSharp.Models;

public class ProxyCredentials
{
    public string Host { get; }
    public int Port { get; }

    public SensitiveString Username { get; }
    public SensitiveString Password { get; }

    /// <summary>
    /// Create new settings for a proxy
    /// </summary>
    /// <param name="host">The proxy hostname/ip</param>
    /// <param name="port">The proxy port</param>
    public ProxyCredentials(string host, int port) : this(host, port, null, (SensitiveString)null)
    {
    }

    /// <summary>
    /// Create new settings for a proxy
    /// </summary>
    /// <param name="host">The proxy hostname/ip</param>
    /// <param name="port">The proxy port</param>
    /// <param name="username">The proxy login</param>
    /// <param name="password">The proxy password</param>
    public ProxyCredentials(string host, int port, string username, string password) : this(host, port, username?.ToSensitiveString(), password?.ToSensitiveString())
    {
    }

    /// <summary>
    /// Create new settings for a proxy
    /// </summary>
    /// <param name="host">The proxy hostname/ip</param>
    /// <param name="port">The proxy port</param>
    /// <param name="username">The proxy login</param>
    /// <param name="password">The proxy password</param>
    public ProxyCredentials(string host, int port, SensitiveString username, SensitiveString password)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
    }
}
