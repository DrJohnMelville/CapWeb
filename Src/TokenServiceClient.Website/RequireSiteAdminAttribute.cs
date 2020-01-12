using Microsoft.AspNetCore.Authorization;

namespace TokenServiceClient.Website
{
    public class RequireSiteAdminAttribute : AuthorizeAttribute
    {
        public RequireSiteAdminAttribute()
        {
            Policy = CapWebTokenNames.AdmiPolicyName;
        }
    }
}