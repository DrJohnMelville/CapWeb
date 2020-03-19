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
using TokenServiceClient.Native.AuthenticateInBrowser;
using TokenServiceClient.Native.PersistentToken;
using TokenServiceClient.Native.RefreshTokenDatabase;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace TokenServiceClient.Native.OAuthClient
{
    public class OAuthClientLogin: ITokenSource
    {
        private readonly string baseUrl;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly SystemBrowser browser;
        private readonly HttpClient web;

        public OAuthClientLogin(string baseUrl, string clientId, string clientSecret, HttpMessageHandler? web = null)
        {
            this.baseUrl = baseUrl;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            browser = new SystemBrowser();
            this.web = new HttpClient(web ?? new HttpClientHandler());
        }

        public string TokenDatabaseKey() => $"OAuth|{baseUrl}|{clientId}";

        public async Task<AccessTokenHolder> Activate(string refreshToken)
        {
            HttpResponseMessage? msg = null;
            if (refreshToken != "")
            {
                msg = await TokenQuery("refresh_token", refreshToken, "refresh_token");
                if (!msg.IsSuccessStatusCode)
                {
                    msg = null;
                }
            }

            if (msg == null)
            {
                msg = await LoginInteractiveAsync();
            }
            
            ThrowIfError(!msg.IsSuccessStatusCode,
                $"Token endpoint call failed with code: {msg.StatusCode}");
            return await CreateCredential(msg);

        }

        private async Task<HttpResponseMessage> LoginInteractiveAsync()
        {
            var result = await browser.InvokeAsync(new BrowserOptions(UserAuthenticationUrl(), "https://127.0.0.1"));
            ThrowIfError(result.IsError, result.Error);

            return await TokenQuery("authorization_code", GetAuthCode(result.Response), "code");
        }

        private Task<HttpResponseMessage> TokenQuery(string requestType, string authCode, string codeFieldName)
        {
            var tokenQuery = ParseBrowserLoginResponse(requestType, authCode, codeFieldName);
            var tokenRequest = web.PostAsync($"{baseUrl}/token", tokenQuery);
            return tokenRequest;
        }

        private static void ThrowIfError(bool isError, string error)
        {
            if (isError)
            {
                throw new TokenAuthenticationException(error);
            }
        }

        private static async Task<AccessTokenHolder> CreateCredential(HttpResponseMessage tokenResult)
        {
            var credential =
                JsonSerializer.Deserialize<OAuth2TokenResponse>((await tokenResult.Content.ReadAsByteArrayAsync()).AsSpan());
            return new AccessTokenHolder(credential.access_token, DateTime.Now.AddSeconds(credential.expires_in),
                credential.refresh_token);
        }

        private FormUrlEncodedContent ParseBrowserLoginResponse(string authCodeType, string authCode, string codeFieldName)
        {
            return new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", authCodeType),
                new KeyValuePair<string, string>(codeFieldName, authCode),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret)
            });
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
}