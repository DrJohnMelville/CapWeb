using Microsoft.Win32;

namespace TokenServiceClient.Native.RefreshTokenDatabase
{
    public class RegistryRefreshTokenDatabase: IRefreshTokenDatabase
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
}