namespace ApiSharp.Authentication;

public class BitcoinAuthenticationProvider : AuthenticationProvider
{
    public BitcoinAuthenticationProvider(string username, string password) : base(new ApiCredentials(username, password))
    {
    }

    public BitcoinAuthenticationProvider(ApiCredentials credentials) : base(credentials)
    {
    }

    public override void AuthenticateRestApi(RestApiClient apiClient, Uri uri, HttpMethod method, bool signed, ArraySerialization arraySerialization, SortedDictionary<string, object> queryParameters, SortedDictionary<string, object> bodyParameters, string bodyContent, SortedDictionary<string, string> headerParameters, Dictionary<string, string> authenticationHeaders)
    {
        throw new NotImplementedException();

        /*
        // Check Point
        if (!auth) return;

        // Action
        var authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(Credentials.Key.GetString() + ":" + Credentials.Secret.GetString()));
        headers.Add("Authorization", "Basic " + authInfo);
        */
    }

    public override void AuthenticateStreamApi()
    {
        throw new NotImplementedException();
    }

    public override void AuthenticateSocketApi()
    {
        throw new NotImplementedException();
    }

}
