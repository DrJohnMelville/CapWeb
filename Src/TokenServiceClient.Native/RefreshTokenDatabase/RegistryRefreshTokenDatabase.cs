using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace TokenServiceClient.Native.RefreshTokenDatabase
{
    public class RegistryRefreshTokenDatabase: IRefreshTokenDatabase
    {
        public bool TryGetToken(string key, [NotNullWhen(true)]out string? token)
        {
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException("Windows Only");
            using var registry = OpenRegistryKey();
            token = registry.GetValue(key) as string;
            return token != null;
        }

        public void PushToken(string key, string token)
        {
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException("Windows Only");
            using var registry = OpenRegistryKey();
            registry.SetValue(key, token);
        }

        private RegistryKey OpenRegistryKey()
        {
            if (!OperatingSystem.IsWindows()) throw new NotSupportedException("Windows Only");
            return Registry.CurrentUser.CreateSubKey(@"SOFTWARE\DrJohnMelville\TokenClientNative\StoredRefreshKeys");
        }
    }
}