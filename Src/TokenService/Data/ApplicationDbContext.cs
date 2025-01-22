using System;
using System.Linq;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Operators;
using TokenService.Configuration.IdentityServer;
using TokenService.Data.ClientData;
using TokenService.Data.UserPriviliges;
using TokenService.Models;

namespace TokenService.Data
{
    
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SigningCredentialData> SigningCredentials { get; set;  } = null!;
        public DbSet<PersistedGrant> PersistedGrants { get; set; } = null!;
        public DbSet<ClientSite> ClientSites { get; set; } = null!;
        public DbSet<UserPrivilege> UserPrivileges { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SigningCredentialData>().HasKey(i => i.KeyId);
            builder.Entity<PersistedGrant>().HasKey(i => i.Key);
            builder.Entity<ClientSite>().HasKey(i => i.ShortName);
            builder.Entity<UserPrivilege>().HasKey(i=>new {i.SiteId, i.UserId});
            builder.Entity<UserPrivilege>().HasOne(i => i.Site).WithMany(i => i.UserPrivileges)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<UserPrivilege>()
                .HasOne(i => i.User).WithMany(i => i.UserPrivileges).OnDelete(DeleteBehavior.Cascade);            
            PatchSqLiteDateTimeOffsets(builder);
        }

        private void PatchSqLiteDateTimeOffsets(ModelBuilder builder)
        {
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset));
                    foreach (var property in properties)
                    {
                        builder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter());
                    }
                }
            }
        }
    }
}
