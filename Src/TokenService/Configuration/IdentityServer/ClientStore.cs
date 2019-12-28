using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
    public interface IInvalidateClients 
    {
        void Invalidate();
    }

    public static class InvalidateClientsOperations
    {
        public static void Invalidate(this IEnumerable<IInvalidateClients> clients)
        {
            foreach (var client in clients)
            {
                client.Invalidate();
            }
        }
    }
    public class ClientStore : IInvalidateClients, IClientStore
    {
        private List<Client> clients = new List<Client>();
        private bool valid;
        private readonly InMemoryClientStore innerStore;
        private readonly Func<ApplicationDbContext> databaseFactory;

        public ClientStore(Func<ApplicationDbContext> databaseFactory)
        {
            this.databaseFactory = databaseFactory;
            innerStore = new InMemoryClientStore(clients);
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            if (!valid)
            {
                await UpdateList();
            }

            return await innerStore.FindClientByIdAsync(clientId);
        }

        public void Invalidate()
        {
            valid = false;
        }

        private async Task UpdateList()
        {
            clients.Clear();
            clients.AddRange((await databaseFactory()
                .ClientSites.AsNoTracking().ToListAsync())
                .SelectMany(i=>i.Clients()));
            valid = true;
        }
    }
}