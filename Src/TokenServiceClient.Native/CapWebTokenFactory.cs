using IdentityModel.OidcClient;

namespace TokenServiceClient.Native
{
    public static class CapWebTokenFactory
    {
        public static CapWebTokenHolder CreateCapWebClient(string clientShortName, string clientSecret)
        {
            string clientName = "web" + clientShortName;
            string desiredScopes = "openid profile offline_access api" + clientShortName;
            var ret = Create("https://capweb.drjohnmelville.com", clientName, clientSecret, desiredScopes);
            return ret;
        }

        public static CapWebTokenHolder Create(string authority, string clientName, string clientSecret, 
            string desiredScopes) => 
            new CapWebTokenHolder(ConfigureClient(authority, clientName, clientSecret, desiredScopes));

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