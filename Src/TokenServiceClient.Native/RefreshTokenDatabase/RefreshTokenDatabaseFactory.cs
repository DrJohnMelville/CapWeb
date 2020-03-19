using System.Diagnostics.CodeAnalysis;

namespace TokenServiceClient.Native.RefreshTokenDatabase
{
    public interface IRefreshTokenDatabase
    {
        public bool TryGetToken(string key, [NotNullWhen(true)] out string? token);
        public void PushToken(string key, string token);
    }

    public static class RefreshTokenDatabaseFactory
    {
        public static IRefreshTokenDatabase Create()=> 
            new EncryptedRefreshDatabase(
                new RegistryRefreshTokenDatabase()
                );
    }
}