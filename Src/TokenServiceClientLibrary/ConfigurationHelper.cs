using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace TokenServiceClientLibrary
{
    public static class CapWebTokenNames
    {
        public const string AdmiPolicyName = "Administrator";
    }
    public class RequireSiteAdminAttribute : AuthorizeAttribute
    {
        public RequireSiteAdminAttribute()
        {
            Policy = CapWebTokenNames.AdmiPolicyName;
        }
    }
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

            services.AddAuthorization(options =>
            {
                options.AddPolicy(CapWebTokenNames.AdmiPolicyName,
                    policy => policy.RequireClaim("role", "Administrator"));
            });
        }

        public static void AddCapWebAuthentication(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}