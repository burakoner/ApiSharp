namespace ApiSharp.Authentication;

/// <summary>
/// Credentials type
/// </summary>
public enum ApiCredentialsType
{
    /// <summary>
    /// HMAC keys credentials
    /// </summary>
    HMAC,

    /// <summary>
    /// RSA keys credentials in xml format
    /// </summary>
    RsaXml,

    /// <summary>
    /// Rsa keys credentials in pem/base64 format. Only available for .NetStandard 2.1 and up, use xml format for lower.
    /// </summary>
    RsaPem,

    /// <summary>
    /// Ed25519 keys credentials in base64 format. Only available for .Net 8.0 and up.
    /// </summary>
    Ed25519,
}
