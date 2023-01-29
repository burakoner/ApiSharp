namespace ApiSharp.Authentication;

public class ApiCredentials: IDisposable
{
    public SensitiveString Key { get; }
    public SensitiveString Secret { get; }

    public ApiCredentials(SensitiveString key, SensitiveString secret)
    {
        if (key == null || secret == null)
            throw new ArgumentException("Key and secret can't be null/empty");

        Key = key;
        Secret = secret;
    }

    public ApiCredentials(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key can't be null/empty");

        Key = key.ToSensitiveString();
        Secret = string.Empty.ToSensitiveString();
    }

    public ApiCredentials(string key, string secret)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            throw new ArgumentException("Key and secret can't be null/empty");

        Key = key.ToSensitiveString();
        Secret = secret.ToSensitiveString();
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
