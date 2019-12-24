using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
  public sealed class PersistentGrantsStore: IPersistedGrantStore
  {
    private readonly ApplicationDbContext db;

    public PersistentGrantsStore(ApplicationDbContext db)
    {
      this.db = db;
    }

    public async Task StoreAsync(PersistedGrant grant)
    {
      if ((await GetAsync(grant.Key)) != null)
      {
        db.PersistedGrants.Update(grant);
      }
      else
      {
        db.PersistedGrants.Add(grant);
      }
      await db.SaveChangesAsync();
    }

    public Task<PersistedGrant> GetAsync(string key) =>
          db.PersistedGrants.FindAsync(key).AsTask();

    public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId) =>
      await db.PersistedGrants.AsNoTracking().Where(i => i.SubjectId == subjectId).ToListAsync();
 
    public  Task RemoveAsync(string key) => InnerRemove(i => i.Key == key);

    private async Task InnerRemove(Expression<Func<PersistedGrant, bool>> predicate)
    {
      var items = await db.PersistedGrants.Where(predicate).ToListAsync();
      db.PersistedGrants.RemoveRange(items);
      await db.SaveChangesAsync();
    }

    public Task RemoveAllAsync(string subjectId, string clientId) =>
      InnerRemove(i => i.ClientId == clientId && i.SubjectId == subjectId);


    public Task RemoveAllAsync(string subjectId, string clientId, string type) =>
      InnerRemove(i => i.ClientId == clientId && i.SubjectId == subjectId && i.Type == type);
  }
}