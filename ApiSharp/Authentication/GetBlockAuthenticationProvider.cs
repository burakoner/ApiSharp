namespace ApiSharp.Authentication;

public class GetBlockAuthenticationProvider : AuthenticationProvider
{

    public GetBlockAuthenticationProvider(string apikey) : base(new ApiCredentials(apikey))
    {
    }

    public GetBlockAuthenticationProvider(ApiCredentials credentials) : base(credentials)
    {
    }

    public override void AuthenticateRestApi(RestApiClient apiClient, Uri uri, HttpMethod method, bool signed, ArraySerialization serialization, SortedDictionary<string, object> query, SortedDictionary<string, object> body, string bodyContent, SortedDictionary<string, string> headers)
    {
        // Check Point
        if (!signed) return;

        // Action
        headers.Add("x-api-key", Credentials.Key.GetString());
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
