using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;

namespace TokenService.Configuration.IdentityServer
{
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
}