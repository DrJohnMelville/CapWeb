using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TokenService.Data;

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
                provider.CreateScope().ServiceProvider.GetRequiredService<ApplicationDbContext>());

        }
    }
}