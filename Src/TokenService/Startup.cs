// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.IO;
using AspNetCoreLocalLog.EmailExceptions;
using AspNetCoreLocalLog.LogSink;
using IdentityServer4.Quickstart.UI;
using TokenService.Data;
using TokenService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendMailService;
using Serilog;
using Serilog.Events;
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
            IisConfiguration(services);
            
            services.AddLogRetrieval();
            services.AddSerilogLogger(null);
            services.AddExceptionLogger();

            services.AddApplicationDatabaseAndFactory(Configuration.GetConnectionString("CapWebConnection") 
              ?? throw new InvalidDataException("No Database Connection string"));
         
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
     
            services.AddSendEmailService();
           services.AddTransient<IPasswordResetNotificationSender, PasswordResetNotificationSender>();
            services.AddTokenServer();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Join(Environment.ContentRootPath,
                    "DataProtectionKeys")));

            
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
            app.UseLogRetrieval()!.WithSecret(Configuration.GetSection("LogRetrieval:Secret").Value??"");
            ShowMoreErrorsInDevelopmentEnviornment(app);

            EnforceHttpsConnectionsOnly(app);

            app.UseSerilogRequestLogging();
            
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
                app.UseMigrationsEndPoint();
            }

            app.UseExceptionLogger().WithEmailTarget("johnmelville@gmail.com");
        }
    }
}