using System;
using System.Linq;
using System.Security.Claims;

namespace TokenServiceClient.Website
{
    public static class ClaimsPrincipleOperations
    {
        public static string SubjectId(this ClaimsPrincipal prin) => prin.ClaimValue("sub");
        public static string Role(this ClaimsPrincipal prin) => prin.ClaimValue("role");
        public static string Name(this ClaimsPrincipal prin) => prin.ClaimValue("name");
        public static string Email(this ClaimsPrincipal prin) => prin.ClaimValue("email");
        public static string ClaimValue(this ClaimsPrincipal prin, string claim) => 
          prin.Claims.FirstOrDefault(i=>i.Type.Equals(claim, StringComparison.Ordinal))?.Value??"";
        public static bool IsSiteAdministrator(this ClaimsPrincipal prin) => 
            "Administrator".Equals(prin.Role(), StringComparison.Ordinal);
    }
}