using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using IdentityServer4.Models;
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
        await db.PersistedGrants.AddAsync(grant);
      }
      await db.SaveChangesAsync();
    }

    public Task<PersistedGrant> GetAsync(string key) =>
          db.PersistedGrants.FindAsync(key).AsTask();

   public async Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
      return await db.PersistedGrants.AsNoTracking().Filter(filter).ToListAsync();
    }

    public Task RemoveAllAsync(PersistedGrantFilter filter)
    {
      return InnerRemoveList(db.PersistedGrants.Filter(filter));
    }

    public  Task RemoveAsync(string key) => InnerRemoveList(db.PersistedGrants.Where(i => i.Key == key));

    private async Task InnerRemoveList(IQueryable<PersistedGrant> listTask)
    {
      db.PersistedGrants.RemoveRange(await listTask.ToListAsync());
      await db.SaveChangesAsync();
    }

    
  }

  public static class PersistedGrantFilterOperation
  {
    public static IQueryable<PersistedGrant> Filter(this IQueryable<PersistedGrant> query, PersistedGrantFilter filter)
    {
      return query
        .FilterByIfNotWhitespace(filter.ClientId, i => i.ClientId == filter.ClientId)
        .FilterByIfNotWhitespace(filter.SessionId, i => i.SessionId == filter.SessionId)
        .FilterByIfNotWhitespace(filter.SubjectId, i => i.SubjectId == filter.SubjectId)
        .FilterByIfNotWhitespace(filter.Type, i => i.Type == filter.Type);
    }

    private static IQueryable<T> FilterByIfNotWhitespace<T>(this IQueryable<T> source, string? key,
      Expression<Func<T, bool>> selector) =>
      string.IsNullOrWhiteSpace(key) ? source : source.Where(selector);
  }
}