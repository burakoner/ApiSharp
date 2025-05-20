namespace ApiSharp;

/// <summary>
/// Rest API Constants
/// </summary>
public class RestApiConstants
{
    /// <summary>
    /// Http Client User Agent
    /// </summary>
    public const string USER_AGENT = "ApiSharp/3.7.0";

    /// <summary>
    /// Json content type header
    /// </summary>
    public const string JSON_CONTENT_HEADER = "application/json";

    /// <summary>
    /// Text content type header
    /// </summary>
    public const string TEXT_CONTENT_HEADER = "text/plain";

    /// <summary>
    /// Form content type header
    /// </summary>
    public const string FORM_CONTENT_HEADER = "application/x-www-form-urlencoded";

    /// <summary>
    /// Flag for Request Body
    /// </summary>
    public const string RequestBodyParameterKey  = "__BODY__";

    /// <summary>
    /// Flag for Request Body Empty Content
    /// </summary>
    public const string RequestBodyEmptyContent  = "";
}