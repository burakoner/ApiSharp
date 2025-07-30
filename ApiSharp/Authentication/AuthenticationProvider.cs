#if NET8_0_OR_GREATER
using NSec.Cryptography;
#endif

namespace ApiSharp.Authentication;

/// <summary>
/// AuthenticationProvider
/// </summary>
public abstract class AuthenticationProvider
{
    /// <summary>
    /// Access Credentials
    /// </summary>
    public ApiCredentials Credentials { get; }

    /// <summary>
    /// Byte representation of the secret
    /// </summary>
    protected byte[] SecretBytes { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="credentials"></param>
    /// <exception cref="ArgumentException"></exception>
    protected AuthenticationProvider(ApiCredentials credentials)
    {
        if (credentials.Key == null || credentials.Secret == null)
            throw new ArgumentException("Api Key/Secret needed");

        Credentials = credentials;
        SecretBytes = Encoding.UTF8.GetBytes(credentials.Secret.GetString());
    }

    /// <summary>
    /// Authenticate a request. Output parameters should include the providedParameters input
    /// </summary>
    /// <param name="apiClient">The Api client sending the request</param>
    /// <param name="uri">The uri for the request</param>
    /// <param name="method">The method of the request</param>
    /// <param name="signed">If the requests should be authenticated</param>
    /// <param name="serialization">Array serialization type</param>
    /// <param name="query">Parameters that need to be in the Uri of the request. Should include the provided parameters if they should go in the uri</param>
    /// <param name="body">Parameters that need to be in the body of the request. Should include the provided parameters if they should go in the body</param>
    /// <param name="bodyContent">The body content of the request</param>
    /// <param name="headers">Additional headers to send with the request</param>
    public abstract void AuthenticateRestApi(
        RestApiClient apiClient,
        Uri uri,
        HttpMethod method,
        bool signed,
        ArraySerialization serialization,
        SortedDictionary<string, object> query,
        SortedDictionary<string, object> body,
        string bodyContent,
        SortedDictionary<string, string> headers);

    #region Timestamp Methods
    /// <summary>
    /// Get current timestamp including the time sync offset from the api client
    /// </summary>
    /// <param name="apiClient"></param>
    /// <returns></returns>
    protected static DateTime GetTimestamp(RestApiClient apiClient)
    {
        return DateTime.UtcNow.Add(apiClient?.GetTimeOffset() ?? TimeSpan.Zero)!;
    }

    /// <summary>
    /// Get millisecond timestamp as a string including the time sync offset from the api client
    /// </summary>
    /// <param name="apiClient"></param>
    /// <returns></returns>
    protected static string GetMillisecondTimestamp(RestApiClient apiClient)
    {
        return GetTimestamp(apiClient).ConvertToMilliseconds().ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Get millisecond timestamp as a long including the time sync offset from the api client
    /// </summary>
    /// <param name="apiClient"></param>
    /// <returns></returns>
    protected long GetMillisecondTimestampLong(RestApiClient apiClient)
    {
        return GetTimestamp(apiClient).ConvertToMilliseconds();
    }
    #endregion

    #region Signature Methods
    /// <summary>
    /// SHA256 sign the data and return the bytes
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected static byte[] SignSHA256Bytes(string data)
    {
        using var encryptor = SHA256.Create();
        return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// SHA256 sign the data and return the bytes
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected static byte[] SignSHA256Bytes(byte[] data)
    {
        using var encryptor = SHA256.Create();
        return encryptor.ComputeHash(data);
    }

    /// <summary>
    /// SHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA256(string data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA256.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA256(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA256.Create();
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA384(string data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA384.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA384(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA384.Create();
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <returns></returns>
    protected static byte[] SignSHA384Bytes(string data)
    {
        using var encryptor = SHA384.Create();
        return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// SHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <returns></returns>
    protected static byte[] SignSHA384Bytes(byte[] data)
    {
        using var encryptor = SHA384.Create();
        return encryptor.ComputeHash(data);
    }

    /// <summary>
    /// SHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA512(string data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA512.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA512(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = SHA512.Create();
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <returns></returns>
    protected static byte[] SignSHA512Bytes(string data)
    {
        using var encryptor = SHA512.Create();
        return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// SHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <returns></returns>
    protected static byte[] SignSHA512Bytes(byte[] data)
    {
        using var encryptor = SHA512.Create();
        return encryptor.ComputeHash(data);
    }

    /// <summary>
    /// MD5 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignMD5(string data, SignatureOutputType? outputType = null)
    {
        using var encryptor = MD5.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// MD5 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignMD5(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = MD5.Create();
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// MD5 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <returns></returns>
    protected static byte[] SignMD5Bytes(string data)
    {
        using var encryptor = MD5.Create();
        return encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// HMACSHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA256(string data, SignatureOutputType? outputType = null)
        => SignHMACSHA256(Encoding.UTF8.GetBytes(data), outputType);

    /// <summary>
    /// HMACSHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA256(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = new HMACSHA256(SecretBytes);
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// HMACSHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA384(string data, SignatureOutputType? outputType = null)
        => SignHMACSHA384(Encoding.UTF8.GetBytes(data), outputType);

    /// <summary>
    /// HMACSHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA384(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = new HMACSHA384(SecretBytes);
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// HMACSHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA512(string data, SignatureOutputType? outputType = null)
        => SignHMACSHA512(Encoding.UTF8.GetBytes(data), outputType);

    /// <summary>
    /// HMACSHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA512(byte[] data, SignatureOutputType? outputType = null)
    {
        using var encryptor = new HMACSHA512(SecretBytes);
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA256 sign the data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    protected string SignRSASHA256(byte[] data, SignatureOutputType? outputType = null)
    {
        using var rsa = CreateRSA();
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA384 sign the data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    protected string SignRSASHA384(byte[] data, SignatureOutputType? outputType = null)
    {
        using var rsa = CreateRSA();
        using var sha384 = SHA384.Create();
        var hash = sha384.ComputeHash(data);
        var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA512 sign the data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outputType"></param>
    /// <returns></returns>
    protected string SignRSASHA512(byte[] data, SignatureOutputType? outputType = null)
    {
        using var rsa = CreateRSA();
        using var sha512 = SHA512.Create();
        var hash = sha512.ComputeHash(data);
        var resultBytes = rsa.SignHash(hash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// Sign data using Ed25519 algorithm
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outputType"></param>
    /// <param name="keyEncoding"></param>
    /// <param name="dataEncoding"></param>
    /// <returns></returns>
    protected string SignEd25519(string data, SignatureOutputType? outputType = null, Encoding? keyEncoding = null, Encoding? dataEncoding = null)
        => SignEd25519((dataEncoding ?? Encoding.ASCII).GetBytes(data), outputType, keyEncoding);

    /// <summary>
    /// Sign data using Ed25519 algorithm
    /// </summary>
    /// <param name="data"></param>
    /// <param name="outputType"></param>
    /// <param name="keyEncoding"></param>
    /// <returns></returns>
    protected string SignEd25519(byte[] data, SignatureOutputType? outputType = null, Encoding? keyEncoding = null)
    {
#if NET8_0_OR_GREATER
        // Algorithm
        var algorithm = SignatureAlgorithm.Ed25519;

        // Import Key
        var secret = Credentials.Secret!.GetString()
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Trim();
        using var key = Key.Import(algorithm, (keyEncoding ?? Encoding.ASCII).GetBytes(secret), KeyBlobFormat.PkixPrivateKeyText);

        // Signature
        var signatureBytes = algorithm.Sign(key, data);

        // Return
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(signatureBytes) : BytesToHexString(signatureBytes);
#else
        throw new NotSupportedException("Ed25519 Algorithm is supported only .Net 8.0 or greater.");
#endif
    }
    #endregion

    #region RSA Methods
    private RSA CreateRSA()
    {
        var rsa = RSA.Create();
        if (Credentials.Type == ApiCredentialsType.RsaPem)
        {
#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
            // Read from pem private key
            var key = Credentials.Secret!.GetString()
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Trim();
            rsa.ImportPkcs8PrivateKey(Convert.FromBase64String(key), out _);
#else
                throw new Exception("Pem format not supported when running from .NetStandard2.0. Convert the private key to xml format.");
#endif
        }
        else if (Credentials.Type == ApiCredentialsType.RsaXml)
        {
            // Read from xml private key format
            rsa.FromXmlString(Credentials.Secret!.GetString());
        }
        else
        {
            throw new Exception("Invalid credentials type");
        }

        return rsa;
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Convert byte array to hex string
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    protected static string BytesToHexString(byte[] buff)
    {
#if NET9_0_OR_GREATER
            return Convert.ToHexString(buff);
#else
        var result = string.Empty;
        foreach (var t in buff) result += t.ToString("X2");
        return result;
#endif
    }

    /// <summary>
    /// Convert byte array to base64 string
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    protected static string BytesToBase64String(byte[] buff)
    {
        return Convert.ToBase64String(buff);
    }
    #endregion
}