using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
    public class ResourceStore:IResourceStore, IInvalidateClients
    {
        private readonly InMemoryResourcesStore store;

        private readonly List<IdentityResource> identityResources = new List<IdentityResource>()
        {
            new IdentityResources.OpenId(),
            new IdentityResource("profile", new []{JwtClaimTypes.Name, JwtClaimTypes.Email, JwtClaimTypes.Role})
        };
        private readonly List<ApiResource> apiResources = new List<ApiResource>();
        private readonly Func<ApplicationDbContext> dbFactory;
        private bool validStore;
        
        public ResourceStore(Func<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
            store = new InMemoryResourcesStore(identityResources, apiResources);
        }

        private ValueTask EnsureValid()
        {
            return validStore ? new ValueTask() : new ValueTask(LoadContexxt());
        }

        private async Task LoadContexxt()
        {
            using var db = dbFactory();
            var resources = (await db.ClientSites.AsNoTracking()
                    .ToListAsync())
                .SelectMany(i=>i.ApiResource());
            UpdateResourcesAtomic(resources);
        }

        private void UpdateResourcesAtomic(IEnumerable<ApiResource> resources)
        {
            lock (apiResources)
            {
                apiResources.Clear();
                apiResources.AddRange(resources);
                validStore = true;
            }
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            await EnsureValid();
            return await store.FindIdentityResourcesByScopeAsync(scopeNames);
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            await EnsureValid();
            return await store.FindApiResourcesByScopeAsync(scopeNames);
        }

        public async Task<ApiResource> FindApiResourceAsync(string name)
        {
            await EnsureValid();
            return await store.FindApiResourceAsync(name);
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            await EnsureValid();
            return await store.GetAllResourcesAsync();
        }

        public void Invalidate()
        {
            validStore = false;
        }
    }
}