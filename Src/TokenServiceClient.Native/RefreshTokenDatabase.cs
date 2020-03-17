using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Win32;

namespace TokenServiceClient.Native
{
    public interface IRefreshTokenDatabase
    {
        public bool TryGetToken(string key, [NotNullWhen(true)] out string? token);
        public void PushToken(string key, string token);
    }

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

        public bool TryGetToken(string key, out string? token)
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
    public class RefreshTokenDatabase: IRefreshTokenDatabase
    {
        public bool TryGetToken(string key, out string? token)
        {
            using var registry = OpenRegistryKey();
            token = registry.GetValue(key) as string;
            return token != null;
        }

        public void PushToken(string key, string token)
        {
            using var registry = OpenRegistryKey();
            registry.SetValue(key, token);
        }

        private RegistryKey OpenRegistryKey() =>
            Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DrJohnMelville\TokenClientNative\StoredRefreshKeys");
    }

    public static class RefreshTokenDatabaseFactory
    {
        public static IRefreshTokenDatabase Create()=> 
            new EncryptedRefreshDatabase(
                new RefreshTokenDatabase()
                );
    }
}