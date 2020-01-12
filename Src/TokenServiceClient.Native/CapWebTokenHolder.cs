using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace TokenServiceClient.Native
{
  public sealed class CapWebTokenHolder
  {
    private readonly LoginResult result;

    public CapWebTokenHolder(LoginResult result)
    {
      this.result = result;
    }

    public void AddBearerToken(HttpClient client)
    {
      client.SetBearerToken(result.AccessToken);
    }

    public static async Task<CapWebTokenHolder> Authenticate(string clientShortName, string clientSecret) =>
      new CapWebTokenHolder(
        await new OidcClient(ConfigureClient(clientShortName, clientSecret)).LoginAsync());

    private static OidcClientOptions ConfigureClient(string clientShortName, string clientSecret)
    {
      var systemBrowser = new SystemBrowser();      // cannot inline because it is used twice below
      var options = new OidcClientOptions
      {
        Authority = "https://capweb.drjohnmelville.com",
        ClientId = "web" + clientShortName,
        RedirectUri = systemBrowser.RedirectUri,
        ClientSecret = clientSecret,
        Scope = "openid profile api" + clientShortName,
        Browser = systemBrowser,
        Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
        ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
      };
      return options;
    }
  }
}