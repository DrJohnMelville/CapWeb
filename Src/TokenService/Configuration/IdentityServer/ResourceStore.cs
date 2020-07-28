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
        private readonly List<ApiScope> apiScopes = new List<ApiScope>();
        private readonly Func<ApplicationDbContext> dbFactory;
        private bool validStore;
        
        public ResourceStore(Func<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
            store = new InMemoryResourcesStore(identityResources, apiResources, apiScopes);
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
//                apiResources.AddRange(resources);
                apiScopes.Clear();
                apiScopes.AddRange(resources.Select(i=>new ApiScope(i.Name, i.UserClaims)));
                validStore = true;
            }
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            await EnsureValid();
            return await store.FindIdentityResourcesByScopeNameAsync(scopeNames);
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            await EnsureValid();
            return await store.FindApiResourcesByScopeNameAsync(scopeNames);
        }

        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            await EnsureValid();
            return await store.FindApiScopesByNameAsync(scopeNames);
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            await EnsureValid();
            return await store.FindApiResourcesByNameAsync(apiResourceNames);
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