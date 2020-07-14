using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;

namespace TokenServiceClient.Native.RefreshTokenDatabase
{
    public class EncryptedRefreshDatabase : IRefreshTokenDatabase
    {
        private static IDataProtector crypto = DataProtectionProvider
            .Create("TokenServiceClient")
            .CreateProtector("Encrypt RefreshTokens");
 
        private IRefreshTokenDatabase innerDatabase;

        public EncryptedRefreshDatabase(IRefreshTokenDatabase innerDatabase)
        {
            this.innerDatabase = innerDatabase;
        }

        public bool TryGetToken(string key, [NotNullWhen(true)]out string? token)
        {
            if (innerDatabase.TryGetToken(key, out token))
            {
                token = crypto.Unprotect(token);
                return true;
            }
            return false;
        }

        public void PushToken(string key, string token) => innerDatabase.PushToken(key, crypto.Protect(token));
    }
}