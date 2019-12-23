using System;
using TokenService.Data;

namespace TokenService.Configuration.IdentityServer
{
    public class SigningCredentialStore
    {
        private readonly Func<ApplicationDbContext> dbFactory;

        public SigningCredentialStore(Func<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }
    }
}