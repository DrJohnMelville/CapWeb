using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using TokenService.Data.UserPriviliges;

namespace TokenService.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
          public IList<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
    }
}
