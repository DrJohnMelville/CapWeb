using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
    public class SigningCredentialDatabase
    {
        private readonly Func<ApplicationDbContext> dbFactory;
        private readonly ISystemClock systemClock;

        private SigningCredentials? signingCredential;
        private IEnumerable<SecurityKeyInfo> verificationCredentials = Array.Empty<SecurityKeyInfo>();
        public DateTimeOffset CacheExpiresAt { get; private set; } = DateTime.MinValue;

        public SigningCredentialDatabase(Func<ApplicationDbContext> dbFactory, ISystemClock systemClock)
        {
            this.dbFactory = dbFactory;
            this.systemClock = systemClock;
        }
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            await RecomputeKeysIfNeeded();
            return signingCredential ?? throw new InvalidDataException("Should never get here, null signingCredential");
        }

        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            await RecomputeKeysIfNeeded();
            return verificationCredentials;
        }


        private async Task RecomputeKeysIfNeeded()
        {
            if (CacheExpiresAt <= systemClock.UtcNow)
            {
                await RecomputeKeys();
            }
        }

        private async Task RecomputeKeys()
        {
            await using var db = dbFactory();
            await using var keyComputer = new SigningCredentialCacheUpdater(db,
              await db.SigningCredentials.AsNoTracking().ToListAsync(), systemClock.UtcNow);
            signingCredential = keyComputer.SigningCredentials();
            verificationCredentials = keyComputer.VerificationKeys();
            CacheExpiresAt = keyComputer.NextExpiration();
        }

    }

    public class SigningCredentialCacheUpdater: IAsyncDisposable
    {
        private readonly ApplicationDbContext db;
        private readonly IList<SigningCredentialData> list;
        private readonly DateTimeOffset time;
        private SigningCredentialData? activeCredential;
        private bool databaseNeedsUpdate;
        public SigningCredentials SigningCredentials() => 
            activeCredential?.ToSigningCrendentials() ?? throw new InvalidDataException("No active credential");
        public IList<SecurityKeyInfo> VerificationKeys() => list.Select(i => i.ToSecurityKeyInfo()).ToList();
        public SigningCredentialCacheUpdater(ApplicationDbContext db, IList<SigningCredentialData> list, DateTimeOffset time)
        {
            this.db = db;
            this.list = list;
            this.time = time;
            UpdateList();
        }

        public DateTimeOffset NextExpiration() =>
            list.Select(i => i.EndOfGracePeriodDate())
                .Append(activeCredential?.ExpirationDate()??DateTimeOffset.Now)
                .Min();

        private void UpdateList()
        {
            ComputeActiveCredential();
            RemoveKeys();
        }

        public ValueTask DisposeAsync() => 
            databaseNeedsUpdate ? new ValueTask(db.SaveChangesAsync()) : new ValueTask();

        private void RemoveKeys()
        {
            foreach (var key in ExpiredKeys())
            {
                list.Remove(key);
                db.SigningCredentials.Remove(key);
                databaseNeedsUpdate = false;
            }
        }

        private List<SigningCredentialData> ExpiredKeys() => 
            list.Where(i=>i.EndOfGracePeriodDate() < time).ToList();

        private void ComputeActiveCredential()
        {
            activeCredential = ActiveCredential();
            if (activeCredential != null) return;
            CreateNewCredential();
        }

        private void CreateNewCredential()
        {
            activeCredential = SigningCredentialData.CreateNewCredential(time);
            db.SigningCredentials.Add(activeCredential);
            list.Add(activeCredential);
            databaseNeedsUpdate = true;
        }

        private SigningCredentialData? ActiveCredential() => 
            list.FirstOrDefault(i => time < i.ExpirationDate() );
    }
}