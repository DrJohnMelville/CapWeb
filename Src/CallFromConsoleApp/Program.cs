using System;
using System.IdentityModel.Tokens.Jwt;
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

        private static void DumpToken(string tokenText)
        {
            Console.WriteLine(tokenText);
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(tokenText))
            {
                Console.WriteLine("Not a JWT");
                return;
            }

            var decoded = handler.ReadJwtToken(tokenText);
            Console.WriteLine("  Header");
            foreach (var head in decoded.Header)
            {
                Console.WriteLine($"    {head.Key:20} {head.Value}");
                    
            }            Console.WriteLine("  Header");
            foreach (var claim in decoded.Claims)
            {
                Console.WriteLine($"    {claim.Type:20} {claim.Value}");
                    
            }
        }
    }
}