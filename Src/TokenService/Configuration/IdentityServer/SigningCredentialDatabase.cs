using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
    // :ISigningCredentialStore, IValidationKeysStore

    public class SigningCredentialStore : ISigningCredentialStore
    {
        private readonly SigningCredentialDatabase db;

        public SigningCredentialStore(SigningCredentialDatabase db)
        {
            this.db = db;
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync() =>
            db.GetSigningCredentialsAsync();
    }
    
    public class ValidationKeysStore: IValidationKeysStore
    {
        private readonly SigningCredentialDatabase db;

        public ValidationKeysStore(SigningCredentialDatabase db)
        {
            this.db = db;
        }

        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class SigningCredentialDatabase
    {
        private readonly Func<ApplicationDbContext> dbFactory;
        private readonly ISystemClock systemClock;

        private SigningCredentials? signingCredential;
        DateTime timeOfNextKeyRotation = DateTime.MinValue;
        private List<SecurityKeyInfo>? verificationCredentials;

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

        private async Task RecomputeKeysIfNeeded()
        {
            if (timeOfNextKeyRotation <= systemClock.UtcNow)
            {
                timeOfNextKeyRotation = await RecomputeKeys();
            }
        }

        private async Task<DateTime> RecomputeKeys()
        {
            using (var db = dbFactory())
            {
                var time = systemClock.UtcNow;
                var list = await db.SigningCredentials.ToListAsync();
                signingCredential = ComputeActiveSigningKey(list, time, db).ToSigningCrendentials();
                RemoveKeys(ExpiredKeys(list, time), list, db);
                await db.SaveChangesAsync();
                verificationCredentials = list.Select(i => i.ToSecurityKeyInfo()).ToList();
            }
            return DateTime.Now;
        }

        private static List<SigningCredentialData> ExpiredKeys(List<SigningCredentialData> list, DateTimeOffset time)
        {
            return list.Where(i=>i.EndOfGracePeriodDate() < time).ToList();
        }

        private static void RemoveKeys(List<SigningCredentialData> expiredKeys, List<SigningCredentialData> list, ApplicationDbContext db)
        {
            foreach (var key in expiredKeys)
            {
                list.Remove(key);
                db.SigningCredentials.Remove(key);
            }
        }

        private static SigningCredentialData ComputeActiveSigningKey(List<SigningCredentialData> list, DateTimeOffset time, ApplicationDbContext db)
        {
            var principal = list.FirstOrDefault(i => i.ExpirationDate() > time);
            if (principal == null)
            {
                principal = SigningCredentialData.CreateNewCredential(time.DateTime);
                db.SigningCredentials.Add(principal);
            }

            return principal;
        }
    }
}