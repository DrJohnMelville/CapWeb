using TokenService.Models;
using TokenService.Data.ClientData;

namespace TokenService.Data.UserPriviliges
{
    public enum SitePrivilege
    {
        None = 0,
        User = 1,
        Administrator = 2,
    }

    public class UserPrivilege
    {
        public ClientSite Site { get; set; } = null!;
        public int SiteId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public string ApplicationUserId { get; set; } = null!;
        public SitePrivilege Privilege { get; set; }
    }
}