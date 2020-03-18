using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using TokenServiceClient.Native;

namespace CallFromConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Request Token");


                       
            CapWebTokenHolder holder = CapWebTokenFactory.CreateCapWebClient("CapWeb",
                "7v0ehQkQOsWuzx9bT7hcQludASvUFcD5l5JEdkNDPaM");
            await AttemptLogin(holder);

            var client = holder.AuthenticatedClient(); 
            Console.WriteLine("Access Response: "+
                await (await client.GetAsync("https://CapWeb.Drjohnmelville.com/Home/MyAccess")).Content.ReadAsStringAsync()
            );
            
            Console.WriteLine("Done");
        }

        private static async Task AttemptLogin(CapWebTokenHolder holder)
        {
            if (await holder.LoginAsync())
            {
                Console.WriteLine("Token Obtained: " + holder.AccessToken);
                Console.WriteLine("Token Expiration: " + holder.ExpiresAt);
            }
            else
            {
                Console.WriteLine("Login Failed");
            }
        }
    }
}