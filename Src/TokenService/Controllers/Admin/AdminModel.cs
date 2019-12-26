using System;
using System.Collections.Generic;
using TokenService.Models;

namespace TokenService.Controllers.Admin
{
    public class AdminModel
    {
        public IList<ApplicationUser> Users { get; set; } = Array.Empty<ApplicationUser>();
    }
}