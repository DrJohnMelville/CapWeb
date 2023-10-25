// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using Melville.IOC.AspNet.RegisterFromServiceCollection;
using Microsoft.IdentityModel.Logging;

namespace TokenService
{
    public class Program
    {
        public static int Main(string[] args)
        {
            #if DEBUG
            IdentityModelEventSource.ShowPII = true;
            #endif
            try
            {
                var host = CreateHostBuilder(args)
                    .Build();
                
                Log.Information("Starting host...");
                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new MelvilleServiceProviderFactory(true))
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
