using Microsoft.AspNetCore.Authorization;

namespace TokenServiceClientLibrary
{
    public class RequireSiteAdminAttribute : AuthorizeAttribute
    {
        public RequireSiteAdminAttribute()
        {
            Policy = CapWebTokenNames.AdmiPolicyName;
        }
    }
}