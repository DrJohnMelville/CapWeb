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



            CapWebTokenHolder holder = await CapWebTokenHolder.Authenticate("CapWeb",
                "7v0ehQkQOsWuzx9bT7hcQludASvUFcD5l5JEdkNDPaM");

            Console.WriteLine("Token Obtained");

            var client = new HttpClient();
            holder.AddBearerToken(client);
            Console.WriteLine("Access Response: "+
                await (await client.GetAsync("https://localhost:5010/Home/MyAccess")).Content.ReadAsStringAsync()
            );
            
            Console.WriteLine("Done");
        }
    }
}