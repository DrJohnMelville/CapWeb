using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using TokenServiceClient.Native.PersistentToken;
using TokenServiceClient.Native.RefreshTokenDatabase;

namespace TokenServiceClient.Native.PersistentToken
{
    public interface ITokenSource
    {
        Task<AccessTokenHolder> Activate(string refreshToken);
        string TokenDatabaseKey();
    }

    public interface IPersistentAccessToken
    {
        ValueTask<AccessTokenHolder> CurrentAccessToken();
    }

    public class PersistentAccessTokenHolder : IPersistentAccessToken
    {
        private readonly IRefreshTokenDatabase tokenDatabase;
        private readonly ITokenSource source;
        private AccessTokenHolder currentHolder = AccessTokenHolder.None;

        public PersistentAccessTokenHolder(IRefreshTokenDatabase tokenDatabase, ITokenSource source)
        {
            this.tokenDatabase = tokenDatabase;
            this.source = source;
        }

        public ValueTask<AccessTokenHolder> CurrentAccessToken() =>
            DateTime.Now.AddSeconds(30) > currentHolder.ExpiresAt
                ? new ValueTask<AccessTokenHolder>(RefreshToken())
                : new ValueTask<AccessTokenHolder>(currentHolder);

        private async Task<AccessTokenHolder> RefreshToken()
        {
            var priorRefreshToken = GetRefreshKey();
            currentHolder = await source.Activate(priorRefreshToken);
            WriteRefreshKey(priorRefreshToken, currentHolder.RefreshToken);
            return currentHolder;
        }

        private string GetRefreshKey() =>
            currentHolder.RefreshToken != "" ? currentHolder.RefreshToken : LookupRefreshKeyInDatabase();

        private string LookupRefreshKeyInDatabase() =>
            tokenDatabase.TryGetToken(source.TokenDatabaseKey(), out var key) ? key : "";
        
        private void WriteRefreshKey(string oldRefreshToken, string refreshToken)
        {
            if (refreshToken != oldRefreshToken)
            {
                tokenDatabase.PushToken(source.TokenDatabaseKey(), refreshToken);
            }
        }

    }

    public static class PersistentTokenSourceOperatios
    {
        public static HttpClient AuthenticatedClient(this IPersistentAccessToken tokenSource,
            HttpMessageHandler? innerHandler = null) =>
            new HttpClient(new AuthenticatedHttpHandler(tokenSource, innerHandler));
        
        [Obsolete("Use AuthenticatedClient -- will automatically handle token expiration.")]
        public static async ValueTask AddBearerToken(this IPersistentAccessToken tokenSource, HttpClient client) => 
            client.SetBearerToken((await tokenSource.CurrentAccessToken()).AccessToken);

        public static async Task<bool> LoginAsync(this IPersistentAccessToken token)
        {
            try
            {
                return (await token.CurrentAccessToken()).AccessToken != "";

            }
            catch (TokenAuthenticationException)
            {
                return false;
            }
        }
    }

}