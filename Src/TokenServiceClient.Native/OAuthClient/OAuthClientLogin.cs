using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityModel.OidcClient.Browser;
using Newtonsoft.Json;
using TokenServiceClient.Native.RefreshTokenDatabase;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TokenServiceClient.Native.OAuthClient
{
    public class OAuthClientLogin
    {
        private readonly string baseUrl;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly SystemBrowser browser;
        private readonly IRefreshTokenDatabase tokenDatabase;
        private readonly HttpClient web;

        public OAuthClientLogin(string baseUrl, string clientId, string clientSecret,
            IRefreshTokenDatabase? tokenDatabase = null, HttpMessageHandler? web = null)
        {
            this.baseUrl = baseUrl;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            browser = new SystemBrowser();
            this.tokenDatabase = tokenDatabase ?? RefreshTokenDatabaseFactory.Create();
            this.web = new HttpClient(web ?? new HttpClientHandler());
        }

        public async Task<OAuthClientCredential> LoginAsync()
        {
            var result = await browser.InvokeAsync(new BrowserOptions(UserAuthenticationUrl(), "https://127.0.0.1"));
            if (result.IsError)
            {
                throw new InvalidOperationException(result.Error);
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", GetAuthCode(result.Response)),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });

            var tokenResult = await web.PostAsync($"{baseUrl}/token", content);
            if (!tokenResult.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Token endpoint call failed with code: {tokenResult.StatusCode}");
            }

            var credential =  JsonSerializer.Deserialize<OAuth2TokenResponse>((await tokenResult.Content.ReadAsByteArrayAsync()).AsSpan());
            return new OAuthClientCredential(baseUrl, clientId, clientSecret, tokenDatabase, credential.access_token,
                credential.refresh_token, DateTime.Now.AddSeconds(credential.expires_in));
        }

        private string GetAuthCode(string result) => Regex.Match(result, @"code=(\S+)").Groups[1].Value;
        private string UserAuthenticationUrl() =>
            $"{baseUrl}/authorize?client_id={clientId}&response_type=code&redirect_uri={browser.RedirectUri}";


        private class OAuth2TokenResponse
        {
            public string access_token { get; set; } = "";
            public int expires_in { get; set; }
            public string refresh_token { get; set; } = "";
            public string token_type { get; set; } = "";
        }
    }

    public class OAuthClientCredential
    {
        private readonly string baseUrl;
        private readonly string clientId;
        private readonly string clientSecret;
        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }
        public DateTime NextExpiration { get; private set; }
        private IRefreshTokenDatabase tokenDB;

        public OAuthClientCredential(string baseUrl, string clientId, string clientSecret, IRefreshTokenDatabase tokenDb, string accessToken, string refreshToken, DateTime nextExpiration)
        {
            this.baseUrl = baseUrl;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            tokenDB = tokenDb;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            NextExpiration = nextExpiration;
        }
    }
    
}