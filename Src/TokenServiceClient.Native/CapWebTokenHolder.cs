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
    private readonly IRefreshTokenDatabase refreshTokenDatabase;
    public string AccessToken { get; private set; } = "";
    // start with a long expired, but valid expiration time so if we try to use it we will re-authentcicate
    public DateTime ExpiresAt { get; private set; } = new DateTime(1975,07,28);
    public CapWebTokenHolder(OidcClientOptions options, IRefreshTokenDatabase? refreshTokenDatabase = null)
    {
      this.options = options;
      this.refreshTokenDatabase = refreshTokenDatabase ?? RefreshTokenDatabaseFactory.Create();
      client = new OidcClient(options);
    }

    [Obsolete("Use AuthenticatedClient -- will automatically handle token expiration.")]
    public void AddBearerToken(HttpClient client) => client.SetBearerToken(AccessToken);
    
    public HttpClient AuthenticatedClient(HttpMessageHandler? innerHandler = null) =>
      new HttpClient(new AuthenticatedHttpHandler(this, innerHandler));
    
    public async Task<bool> LoginAsync()
    {
      if (refreshTokenDatabase.TryGetToken(RefreshTokenKey(), out var refreshToken))
      {
        var loginResponse = await client.RefreshTokenAsync(refreshToken);
        if (!loginResponse.IsError)
        {
          HandleSuccessfulAuthentication(loginResponse.AccessToken, loginResponse.AccessTokenExpiration, 
            loginResponse.RefreshToken);
          return true;
        }
      }
      return await DoUiLogin();
    }

    private async Task<bool> DoUiLogin()
    {
      var loginResponse = await client.LoginAsync();
      if (loginResponse.IsError) return false;
      HandleSuccessfulAuthentication(loginResponse.AccessToken, loginResponse.AccessTokenExpiration, 
        loginResponse.RefreshToken);
      return true;
    }

    private void HandleSuccessfulAuthentication(string accessToken, DateTime tokenExpiration, string newRefreshToken)
    {
      AccessToken = accessToken;
      ExpiresAt = tokenExpiration;
      WriteRefreshKey(refreshTokenDatabase, newRefreshToken);
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

    [Obsolete("Use CapWebTokeFactory.CreateCapWebClient")]
    public static async Task<CapWebTokenHolder> Authenticate(string clientShortName, string clientSecret)
    {
      var ret = CapWebTokenFactory.CreateCapWebClient(clientShortName, clientSecret);
      await ret.LoginAsync();
      return ret;
    }

    #endregion
  }
}