namespace ApiSharp.Authentication;

public class GetBlockAuthenticationProvider : AuthenticationProvider
{

    public GetBlockAuthenticationProvider(string apikey) : base(new ApiCredentials(apikey))
    {
    }

    public GetBlockAuthenticationProvider(ApiCredentials credentials) : base(credentials)
    {
    }

    public override void AuthenticateRestApi(RestApiClient apiClient, Uri uri, HttpMethod method, bool signed, ArraySerialization arraySerialization, SortedDictionary<string, object> queryParameters, SortedDictionary<string, object> bodyParameters, string bodyContent, SortedDictionary<string, string> headerParameters, Dictionary<string, string> authenticationHeaders)
    {
        throw new NotImplementedException();

        /*
        // Check Point
        if (!auth) return;

        // Action
        headers.Add("x-api-key", Credentials.Key.GetString());
        */
    }

    public override void AuthenticateSocketApi()
    {
        throw new NotImplementedException();
    }

    public override void AuthenticateStreamApi()
    {
        throw new NotImplementedException();
    }
}
