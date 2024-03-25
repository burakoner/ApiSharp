namespace ApiSharp.Authentication;

/// <summary>
/// Api Credentials
/// </summary>
public class ApiCredentials : IDisposable
{
    /// <summary>
    /// Api Key
    /// </summary>
    public SensitiveString Key { get; }

    /// <summary>
    /// Api Secret
    /// </summary>
    public SensitiveString Secret { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">Api Key</param>
    /// <param name="secret">Api Secret</param>
    public ApiCredentials(SensitiveString key, SensitiveString secret)
    {
        Key = key ?? string.Empty.ToSensitiveString();
        Secret = secret ?? string.Empty.ToSensitiveString();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public ApiCredentials(string key)
    {
        Key = !string.IsNullOrEmpty(key) ? key.ToSensitiveString() : string.Empty.ToSensitiveString();
        Secret = string.Empty.ToSensitiveString();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">Api Key</param>
    /// <param name="secret">Api Secret</param>
    public ApiCredentials(string key, string secret)
    {
        Key = !string.IsNullOrEmpty(key) ? key.ToSensitiveString() : string.Empty.ToSensitiveString();
        Secret = !string.IsNullOrEmpty(secret) ? secret.ToSensitiveString() : string.Empty.ToSensitiveString();
    }

    /// <summary>
    /// Copy
    /// </summary>
    /// <returns></returns>
    public virtual ApiCredentials Copy()
    {
        // Use .GetString() to create a copy of the SensitiveString
        return new ApiCredentials(Key!.GetString(), Secret!.GetString());
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        Key?.Dispose();
        Secret?.Dispose();
    }
}
