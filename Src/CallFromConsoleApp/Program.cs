using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using TokenServiceClient.Native;
using TokenServiceClient.Native.PersistentToken;

namespace CallFromConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Request Token");


                       
            var holder = CapWebTokenFactory.CreateCapWebClient("CapWeb",
                "7v0ehQkQOsWuzx9bT7hcQludASvUFcD5l5JEdkNDPaM");
            await AttemptLogin(holder);

            var client = holder.AuthenticatedClient(); 
            Console.WriteLine("Access Response: "+
                await (await client.GetAsync("https://CapWeb.Drjohnmelville.com/Home/MyAccess")).Content.ReadAsStringAsync()
            );
            
            Console.WriteLine("Done");
        }

        private static async Task AttemptLogin(IPersistentAccessToken holder)
        {
            if (await holder.LoginAsync())
            {
                var token = await holder.CurrentAccessToken();
                Console.WriteLine("Token Obtained: " + token.AccessToken);
                Console.WriteLine("Token Expiration: " + token.ExpiresAt);
            }
            else
            {
                Console.WriteLine("Login Failed");
            }
        }
    }
}