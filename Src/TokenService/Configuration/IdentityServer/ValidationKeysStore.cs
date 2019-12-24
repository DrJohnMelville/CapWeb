using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace TokenService.Configuration.IdentityServer
{
  public class ValidationKeysStore: IValidationKeysStore
  {
    private readonly SigningCredentialDatabase db;

    public ValidationKeysStore(SigningCredentialDatabase db)
    {
      this.db = db;
    }

    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
    {
      return db.GetValidationKeysAsync();
    }
  }
}