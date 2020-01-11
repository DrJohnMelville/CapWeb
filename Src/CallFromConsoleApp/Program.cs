using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;

namespace CallFromConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Request Token");

            var client = new HttpClient();

            var systemBrowser = new SystemBrowser();
            var options = new OidcClientOptions
            {
                Authority = "https://capweb.drjohnmelville.com",
                ClientId = "webCapWeb",
                RedirectUri = systemBrowser.RedirectUri,
                ClientSecret = "7v0ehQkQOsWuzx9bT7hcQludASvUFcD5l5JEdkNDPaM",
                Scope = "openid profile apiCapWeb",
                Browser = systemBrowser,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect
            };

            var oidClient = new OidcClient(options);

            var token = await oidClient.LoginAsync(new LoginRequest());
            
            Console.WriteLine($"Token: {token.AccessToken}");
            
            client.SetBearerToken(token.AccessToken);

            Console.WriteLine("Access Response: "+
                await (await client.GetAsync("https://localhost:5010/Home/MyAccess")).Content.ReadAsStringAsync()
            );
            
            Console.WriteLine("Done");
        }
    }
}