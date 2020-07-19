using IdentityModel.OidcClient;
using TokenServiceClient.Native.AuthenticateInBrowser;
using TokenServiceClient.Native.OAuthClient;
using TokenServiceClient.Native.PersistentToken;
using TokenServiceClient.Native.RefreshTokenDatabase;

namespace TokenServiceClient.Native
{
    public static class CapWebTokenFactory
    {
        public static IPersistentAccessToken CreateCapWebClient(string clientShortName, string clientSecret)
        {
            string clientName = "web" + clientShortName;
            string desiredScopes = "openid profile offline_access";//" api" + clientShortName;
            var ret = CreateOidcClient("https://capweb.drjohnmelville.com", clientName, clientSecret, desiredScopes);
            return ret;
        }

        public static IPersistentAccessToken CreateOidcClient(string authority, string clientName, string clientSecret, 
            string desiredScopes)
        {
            var tokenHolder = new CapWebTokenHolder(ConfigureClient(authority, clientName, clientSecret, desiredScopes));
      
            return new PersistentAccessTokenHolder(RefreshTokenDatabaseFactory.Create(), tokenHolder);
        }
        
        public static IPersistentAccessToken CreateOauth2Client(string authority, string clientId, string clientSecret)
        => new PersistentAccessTokenHolder(RefreshTokenDatabaseFactory.Create(), 
            new OAuthClientLogin(authority, clientId, clientSecret));



        public static OidcClientOptions ConfigureClient(string authority, string clientId, string clientSecret,
            string desiredScopes)
        {
            var systemBrowser = new SystemBrowser();      // cannot inline because it is used twice below
            var options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                RedirectUri = systemBrowser.RedirectUri,
                ClientSecret = clientSecret,
                Scope = desiredScopes,
                Browser = systemBrowser,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };
            return options;
        }
    }
}