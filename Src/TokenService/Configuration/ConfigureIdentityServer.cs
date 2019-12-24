using IdentityServer4.Stores;
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

            RegisterSigningTokenServer(services);
        }

        private static void RegisterSigningTokenServer(IServiceCollection services)
        {
            services.AddSingleton<SigningCredentialDatabase, SigningCredentialDatabase>();
            services.AddTransient<ISigningCredentialStore, SigningCredentialStore>();
            services.AddTransient<IValidationKeysStore, ValidationKeysStore>();
        }

        public static void UseTokenService(this IApplicationBuilder app) => app.UseIdentityServer();
    }
}