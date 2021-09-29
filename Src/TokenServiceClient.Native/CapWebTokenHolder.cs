using System;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Win32;
using TokenServiceClient.Native.PersistentToken;
using TokenServiceClient.Native.RefreshTokenDatabase;

namespace TokenServiceClient.Native
{ 
  public sealed class CapWebTokenHolder: ITokenSource
  {
    private readonly OidcClientOptions options;
    private readonly OidcClient client;

    public CapWebTokenHolder(OidcClientOptions options)
    {
      this.options = options;
      client = new OidcClient(options);
    }

    public async Task<AccessTokenHolder> Activate(string refreshToken)
    {
      if (refreshToken != "")
      {
        var loginResponse = await client.RefreshTokenAsync(refreshToken);
        if (!loginResponse.IsError)
        {
          return new AccessTokenHolder(loginResponse.AccessToken, loginResponse.AccessTokenExpiration.DateTime,
            loginResponse.RefreshToken);
        }
      }
      return await DoUiLogin();
    }

    public string TokenDatabaseKey() => $"{options.Authority}|{options.ClientId}|{options.Scope}";

    
    private async Task<AccessTokenHolder> DoUiLogin()
    {
      var loginResponse = await client.LoginAsync();
      if (loginResponse.IsError) throw new TokenAuthenticationException("User login failed");
       return new AccessTokenHolder(loginResponse.AccessToken, loginResponse.AccessTokenExpiration.DateTime, 
         loginResponse.RefreshToken);
    }
  }
}