using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using TokenService.Data;
using TokenService.Data.UserPriviliges;

namespace TokenService.Configuration.IdentityServer
{
    public class ProfileService:IProfileService
    {
        private readonly Func<ApplicationDbContext> dbFactory;

        public ProfileService(Func<ApplicationDbContext> dbFactory)
        {
            this.dbFactory = dbFactory;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.AddRequestedClaims(context.Subject.Claims.Append(
                new Claim(JwtClaimTypes.Role, (await RoleFromRequestContext(context)).ToString())));
        }

        private Task<SitePrivilege> RoleFromRequestContext(ProfileDataRequestContext context) => 
            RoleForContext(context.Subject, context.Client);

        private async Task<SitePrivilege> RoleForContext(ClaimsPrincipal subjectPrincipal, Client client)
        {
            var subjectId = subjectPrincipal.Claims.FirstOrDefault(i => i.Type == JwtClaimTypes.Subject)?.Value ?? "none";
            var site = client.ClientId[3..];
            var role = (await dbFactory().UserPrivileges.AsNoTracking()
                           .FirstOrDefaultAsync(i => i.Site.ShortName == site && i.UserId == subjectId))?.Privilege ??
                       SitePrivilege.None;
            return role;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var role = await RoleForContext(context.Subject, context.Client);
            context.IsActive = role == SitePrivilege.Administrator || role == SitePrivilege.User;
        }
    }
}