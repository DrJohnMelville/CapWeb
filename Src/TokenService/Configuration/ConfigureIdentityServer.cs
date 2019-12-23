using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TokenService.Configuration.IdentityServer;
using TokenService.Models;

namespace TokenService.Configuration
{
    public static class ConfigureIdentityServer
    {
        public static void AddTokenServer(this IServiceCollection services)
        {
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddInMemoryIdentityResources(Config.Ids)
                .AddInMemoryApiResources(Config.Apis)
                .AddInMemoryClients(Config.Clients)
                .AddAspNetIdentity<ApplicationUser>();
            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddSingleton<SigningCredentialDatabase, SigningCredentialDatabase>();
        }

        public static void UseTokenService(this IApplicationBuilder app) => app.UseIdentityServer();
    }
}