namespace ApiSharp.Authentication;

public abstract class AuthenticationProvider
{
    public ApiCredentials Credentials { get; }
    protected byte[] SecretBytes { get; }

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
    /// <param name="authenticationHeaders">The headers that should be send with the request</param>
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

    public abstract void AuthenticateTcpSocketApi();

    public abstract void AuthenticateWebSocketApi();


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
    /// SHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA256(string data, SignatureOutputType outputType =  SignatureOutputType.Hex)
    {
        using var encryptor = SHA256.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA384(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = SHA384.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// SHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignSHA512(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = SHA512.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// MD5 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected static string SignMD5(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = MD5.Create();
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// HMACSHA256 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA256(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = new HMACSHA256(SecretBytes);
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// HMACSHA384 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA384(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = new HMACSHA384(SecretBytes);
        var resultBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(data));
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// HMACSHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA512(string data, SignatureOutputType outputType = SignatureOutputType.Hex)
        => SignHMACSHA512(Encoding.UTF8.GetBytes(data), outputType);

    /// <summary>
    /// HMACSHA512 sign the data and return the hash
    /// </summary>
    /// <param name="data">Data to sign</param>
    /// <param name="outputType">String type</param>
    /// <returns></returns>
    protected string SignHMACSHA512(byte[] data, SignatureOutputType outputType = SignatureOutputType.Hex)
    {
        using var encryptor = new HMACSHA512(SecretBytes);
        var resultBytes = encryptor.ComputeHash(data);
        return outputType == SignatureOutputType.Base64 ? BytesToBase64String(resultBytes) : BytesToHexString(resultBytes);
    }

    /// <summary>
    /// Sign a string
    /// </summary>
    /// <param name="toSign"></param>
    /// <returns></returns>
    public virtual string Sign(string toSign)
    {
        return toSign;
    }

    /// <summary>
    /// Sign a byte array
    /// </summary>
    /// <param name="toSign"></param>
    /// <returns></returns>
    public virtual byte[] Sign(byte[] toSign)
    {
        return toSign;
    }

    /// <summary>
    /// Convert byte array to hex string
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    protected static string BytesToHexString(byte[] buff)
    {
        var result = string.Empty;
        foreach (var t in buff)
            result += t.ToString("X2");
        return result;
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
}
