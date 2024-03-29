﻿using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
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
            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddAspNetIdentity<ApplicationUser>()
                .AddPersistedGrantStore<PersistentGrantsStore>()
                .AddAppAuthRedirectUriValidator();

            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IClientStore, ClientStore>();
            services.AddSingleton<IResourceStore, ResourceStore>();
            services.AddSingleton<IEnumerable<IInvalidateClients>>(d =>
                new[]
                {
                    (IInvalidateClients) d.GetRequiredService<IClientStore>(),
                    (IInvalidateClients) d.GetRequiredService<IResourceStore>(),
                });

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