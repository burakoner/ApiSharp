namespace ApiSharp.Security;

/// <summary>
/// https://github.com/dotnet/platform-compat/blob/master/docs/DE0001.md
/// Microsoft'un tavsiyesi üzerine SecureString yerine bu class hazırlandı.
/// </summary>
public class SensitiveString:IDisposable
{
    private readonly Aes _aes;
    private readonly string _key;
    private readonly byte[] _payload;

    /// <summary>
    /// Constructor for SensitiveString. This class encrypts the string using AES encryption.
    /// </summary>
    /// <param name="secret">Secret</param>
    public SensitiveString(string secret)
    {
        this._key = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
        this._aes = Aes.Create("AesManaged");
        this._aes.Mode = CipherMode.CBC;
        this._aes.Padding = PaddingMode.PKCS7;
        this._aes.KeySize = 0x80;
        this._aes.BlockSize = 0x80;

        var keyBytes = Encoding.UTF8.GetBytes(this._key);
        this._aes.Key = keyBytes;
        this._aes.IV = keyBytes;

        var secretBytes = Encoding.UTF8.GetBytes(secret);
        this._payload = _aes.CreateEncryptor().TransformFinalBlock(secretBytes, 0, secretBytes.Length);
    }

    /// <summary>
    /// Get the decrypted string from the encrypted payload.
    /// </summary>
    /// <returns></returns>
    public string GetString()
    {
        byte[] textByte = _aes.CreateDecryptor().TransformFinalBlock(this._payload, 0, this._payload.Length);
        return Encoding.UTF8.GetString(textByte);
    }

    /// <summary>
    /// Get the decrypted string from the encrypted payload.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return this.GetString();
    }

    /// <summary>
    /// Compare the current instance with another SensitiveString instance.
    /// </summary>
    /// <param name="ss"></param>
    /// <returns></returns>
    public bool IsEqualTo(SensitiveString ss)
    {
        return this.GetString().Equals(ss.GetString());
    }

    /// <summary>
    /// Dispose the Aes instance to free up resources.
    /// </summary>
    public void Dispose()
    {
        _aes.Dispose();
    }
}

/// <summary>
/// SensitiveStringExtensions class provides extension methods for the SensitiveString class.
/// </summary>
public static class SensitiveStringExtensions
{
    /// <summary>
    /// Converts a string to a SensitiveString instance.
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static SensitiveString ToSensitiveString(this string @this)
    {
        return new SensitiveString(@this);
    }
}
