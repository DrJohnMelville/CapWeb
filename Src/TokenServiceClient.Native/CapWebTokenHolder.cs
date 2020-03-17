using System;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;

namespace TokenServiceClient.Native
{
  public sealed class CapWebTokenHolder
  {
    private readonly OidcClientOptions options;
    private readonly OidcClient client;
    public string AccessToken { get; private set; } = "";
    public DateTime ExpiresAt { get; private set; }
    public CapWebTokenHolder(OidcClientOptions options)
    {
      this.options = options;
      client = new OidcClient(options);
    }

    public void AddBearerToken(HttpClient client) => client.SetBearerToken(AccessToken);

    public static async Task<CapWebTokenHolder> Authenticate(string authority, string clientName, string clientSecret,
      string desiredScopes)
    {
      var ret = new CapWebTokenHolder(ConfigureClient(authority, clientName, clientSecret, desiredScopes));
      await ret.LoginAsync();
      return ret;
    }

    private async Task LoginAsync()
    {
      var refreshTokenDatabase = RefreshTokenDatabaseFactory.Create();
      if (refreshTokenDatabase.TryGetToken(RefreshTokenKey(), out var refreshToken))
      {
        var ret = await client.RefreshTokenAsync(refreshToken);
        if (!ret.IsError)
        {
          AccessToken = ret.AccessToken;
          ExpiresAt = ret.AccessTokenExpiration;
          WriteRefreshKey(refreshTokenDatabase, ret.RefreshToken);
          return;
        }
      }
      await DoUiLogin(refreshTokenDatabase);
    }

    private async Task DoUiLogin(IRefreshTokenDatabase refreshTokenDatabase)
    {
      var ret = await client.LoginAsync();
      if (ret.IsError) return;
      AccessToken = ret.AccessToken;
      ExpiresAt = ret.AccessTokenExpiration;
      WriteRefreshKey(refreshTokenDatabase, ret.RefreshToken);
    }

    private void WriteRefreshKey(IRefreshTokenDatabase refreshTokenDatabase, string refreshToken)
    {
      if (!string.IsNullOrWhiteSpace(refreshToken))
      {
        refreshTokenDatabase.PushToken(RefreshTokenKey(), refreshToken);
      }
    }

    private string RefreshTokenKey() => $"{options.Authority}|{options.ClientId}|{options.Scope}";

    #region Configuration
    public static Task<CapWebTokenHolder> Authenticate(string clientShortName, string clientSecret) =>
      Authenticate("https://capweb.drjohnmelville.com", "web" + clientShortName, clientSecret, "openid profile offline_access api" + clientShortName);

    private static OidcClientOptions ConfigureClient(string authority, string clientId, string clientSecret,
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
    #endregion
  }
}