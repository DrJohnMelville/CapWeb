// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using TokenService.Data;
using TokenService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenService.Configuration;

namespace TokenService
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });
            
            services.AddApplicationDatabaseAndFactory(Configuration.GetConnectionString("DefaultConnection"));
            
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
                
            services.AddTokenServer();

            // services.AddAuthentication()
            //     .AddGoogle(options =>
            //     {
            //         // register your IdentityServer with Google at https://console.developers.google.com
            //         // enable the Google+ API
            //         // set the redirect URI to http://localhost:5000/signin-google
            //         options.ClientId = "copy client ID from Google here";
            //         options.ClientSecret = "copy client secret from Google here";
            //     });
        }

        public void Configure(IApplicationBuilder app)
        {
            ShowMoreErrorsInDevelopmentEnviornment(app);

            EnforceHttpsConnectionsOnly(app);

            app.UseStaticFiles();
            
            app.UseRouting();
            
            app.UseTokenService();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }

        private static void EnforceHttpsConnectionsOnly(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }

        private void ShowMoreErrorsInDevelopmentEnviornment(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
        }
    }
}