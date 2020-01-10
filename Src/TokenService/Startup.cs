// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using AspNetCoreLocalLog.LogSink;
using IdentityServer4.Quickstart.UI;
using TokenService.Data;
using TokenService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using TokenService.Configuration;
using TokenService.Services.EmailServices;

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
            services.AddLogRetrieval();
            IisConfiguration(services);

            services.AddApplicationDatabaseAndFactory(Configuration.GetConnectionString("CapWebConnection"));
         
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<ISendEmailService, SendEmailService>();
            services.AddTransient<IPasswordResetNotificationSender, PasswordResetNotificationSender>();
            services.AddTokenServer();

            services.AddAuthorization(options => options.AddPolicy("Administrator",
                pb => pb.RequireClaim("email", "johnmelville@gmail.com")));
        }

        private static void IisConfiguration(IServiceCollection services)
        {
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
        }

        public void Configure(IApplicationBuilder app)
        {
            ShowMoreErrorsInDevelopmentEnviornment(app);

            EnforceHttpsConnectionsOnly(app);

            app.UseLogRetrieval(logger => logger
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
            ).WithSecret(Configuration.GetValue<string>("LogRetrieval:Secret"));

            app.UseStaticFiles();
            
            app.UseRouting();
            
            app.UseTokenService();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute().RequireAuthorization();
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