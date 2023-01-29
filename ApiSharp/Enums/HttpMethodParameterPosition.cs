namespace ApiSharp.Enums
{
    /// <summary>
    /// Where the parameters for a HttpMethod should be added in a request
    /// </summary>
    public enum HttpMethodParameterPosition
    {
        /// <summary>
        /// Parameters in body
        /// </summary>
        InBody,

        /// <summary>
        /// Parameters in url
        /// </summary>
        InUri
    }
}
