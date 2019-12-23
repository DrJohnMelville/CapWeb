using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TokenService.Configuration.IdentityServer;
using TokenService.Data;
using TokenService.Models;

namespace TokenService.Configuration
{
    public static class ConfigureDatabase
    {
        public static void AddApplicationDatabaseAndFactory(this IServiceCollection services,
            string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddSingleton<Func<ApplicationDbContext>>(provider => ()=>
                provider.CreateScope().ServiceProvider.GetService<ApplicationDbContext>());

        }
    }
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

            services.AddSingleton<SigningCredentialStore, SigningCredentialStore>();


        }

        public static void UseTokenService(this IApplicationBuilder app) => app.UseIdentityServer();
    }
}