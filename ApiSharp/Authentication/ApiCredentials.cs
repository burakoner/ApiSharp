namespace ApiSharp.Authentication;

public class ApiCredentials : IDisposable
{
    public SensitiveString Key { get; }
    public SensitiveString Secret { get; }

    public ApiCredentials(SensitiveString key, SensitiveString secret)
    {
        Key = key != null ? key : string.Empty.ToSensitiveString();
        Secret = secret != null ? secret : string.Empty.ToSensitiveString();
    }

    public ApiCredentials(string key)
    {
        Key = !string.IsNullOrEmpty(key) ? key.ToSensitiveString() : string.Empty.ToSensitiveString();
        Secret = string.Empty.ToSensitiveString();
    }

    public ApiCredentials(string key, string secret)
    {
        Key = !string.IsNullOrEmpty(key) ? key.ToSensitiveString() : string.Empty.ToSensitiveString();
        Secret = !string.IsNullOrEmpty(secret) ? secret.ToSensitiveString() : string.Empty.ToSensitiveString();
    }

    public virtual ApiCredentials Copy()
    {
        // Use .GetString() to create a copy of the SensitiveString
        return new ApiCredentials(Key!.GetString(), Secret!.GetString());
    }

    public void Dispose()
    {
        Key?.Dispose();
        Secret?.Dispose();
    }
}
