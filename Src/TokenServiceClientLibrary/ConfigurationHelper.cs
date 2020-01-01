using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace TokenServiceClientLibrary
{
    public static class ConfigurationHelper
    {
        /// <summary>
        /// Adds the required services to accept jwt tokens from CapWeb.Drjohnmelville.com.  This method is called
        /// in ConfigureServices to initalize the AuthenticationService.  You also need to call
        /// app.UseCapWebTokenServices between the app.userouting and app.useEndPoints in the Configure method.
        /// </summary>
        /// <param name="services">The service collection to be added to</param>
        /// <param name="clientId">The ide being used as registered at capweb.drjohnmelville.com</param>
        /// <param name="clientSecret">The client secret, also obtained from CapWeb.DrJohnMelville.com</param>
        public static void AddCapWebTokenService(this IServiceCollection services, string clientId, string clientSecret)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            RegisterCookieAndOpenIdAuthentication(services, clientId, clientSecret);

            RegisterAdministratorPolicy(services);
            
            RegistrClaimPrincipal(services);
        }

        private static void RegisterCookieAndOpenIdAuthentication(IServiceCollection services, string clientId,
            string clientSecret)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://CapWeb.DrJohnMelville.Com";
                    options.RequireHttpsMetadata = false;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                });
        }

        private static void RegisterAdministratorPolicy(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(CapWebTokenNames.AdmiPolicyName,
                    policy => policy.RequireClaim("role", "Administrator"));
            });
        }

        private static void RegistrClaimPrincipal(IServiceCollection services)
        {
            services.AddTransient<ClaimsPrincipal>(s =>
                s.GetService<IHttpContextAccessor>().HttpContext.User);
        }

        public static void AddCapWebAuthentication(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}