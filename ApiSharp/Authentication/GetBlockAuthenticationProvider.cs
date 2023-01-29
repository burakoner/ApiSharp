namespace ApiSharp.Authentication;

public class GetBlockAuthenticationProvider : AuthenticationProvider
{

    public GetBlockAuthenticationProvider(string apikey) : base(new ApiCredentials(apikey))
    {
    }

    public GetBlockAuthenticationProvider(ApiCredentials credentials) : base(credentials)
    {
    }

    public override void AuthenticateRestApi(RestApiClient apiClient, Uri uri, HttpMethod method, Dictionary<string, object> providedParameters, bool auth, ArraySerialization arraySerialization, HttpMethodParameterPosition parameterPosition, out SortedDictionary<string, object> uriParameters, out SortedDictionary<string, object> bodyParameters, out Dictionary<string, string> headers)
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
