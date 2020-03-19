using System;

namespace TokenServiceClient.Native.PersistentToken
{
    public class AccessTokenHolder
    {
        public string AccessToken { get; }
        public DateTime ExpiresAt { get; }
        public string RefreshToken { get; }

        public AccessTokenHolder(string accessToken, DateTime expiresAt, string refreshToken)
        {
            AccessToken = accessToken;
            ExpiresAt = expiresAt;
            RefreshToken = refreshToken;
        }
        public static readonly AccessTokenHolder None = new AccessTokenHolder("", new DateTime(1975,07,28), "");
    }
}