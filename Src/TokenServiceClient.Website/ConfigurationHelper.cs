﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace TokenServiceClient.Website
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
            services.AddHttpContextAccessor();
            services.AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = "Cookies";
                    opt.DefaultSignInScheme = "Cookies";
                    opt.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://capweb.drjohnmelville.com";
                    options.RequireHttpsMetadata = false;
                    options.Audience = $"api{clientId}";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        AudienceValidator = SubstituteScopeForAudienceClaim,
                        ValidateAudience = true,
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://capweb.drjohnmelville.com";
                    options.RequireHttpsMetadata = false;
                    options.ClientId = "web"+clientId;
                    options.ClientSecret = clientSecret;
                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
                });

        }

        private static bool SubstituteScopeForAudienceClaim(IEnumerable<string> audiences, SecurityToken securitytoken, TokenValidationParameters validationparameters)
        {
            return securitytoken switch
            {
                JwtSecurityToken jwt => jwt.Claims.Any(IsRequiredScope),
                JsonWebToken jwt => jwt.Claims.Any(IsRequiredScope),
                _=> false
            };

             bool IsRequiredScope(Claim i) =>
                i.Type.Equals("scope", StringComparison.Ordinal) &&
                i.Value.Equals(validationparameters.ValidAudience, StringComparison.Ordinal);
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
                s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());
        }

        public static void AddCapWebAuthentication(this IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseMiddleware<MultiAuthenticationMiddleware>();
            app.UseAuthorization();
        }
    }
}