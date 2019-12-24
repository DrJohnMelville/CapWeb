using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokenService.Models;

namespace TokenService.Controllers.Users
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor contextFactory;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public UserController(IHttpContextAccessor contextFactory, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager)
        {
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        // GET
        public IActionResult Index()
        {
            return View(new EditUserModel(CurrentUser().Claims));
        }

        private ClaimsPrincipal CurrentUser() => contextFactory.HttpContext.User;

        [HttpPost]
        public async Task<IActionResult> Index(EditUserModel model, string button)
        {
            if (ModelState.IsValid)
            {
                switch (button)
                {
                    case "name":
                        await ChangeName(model);
                        break;
                    case "password":
                        await ChangePassword(model);
                        break;
                }
            }
            return View(model);
        }
        private async Task ChangeName(EditUserModel model)
        {
            var user = await userManager.GetUserAsync(CurrentUser());
            await AddOrReplaceClaim(model, user, new Claim(JwtClaimTypes.Name, model.FullName));
        }

        private async Task AddOrReplaceClaim(EditUserModel model, ApplicationUser user, Claim claim)
        {
            var oldClaim = (await userManager.GetClaimsAsync(user))
                .FirstOrDefault(i => i.Type.Equals(claim.Type, StringComparison.Ordinal));
            if (oldClaim != null)
            {
                await userManager.ReplaceClaimAsync(user, oldClaim, claim);
            }
            else
            {
                await userManager.AddClaimAsync(user, claim);
            }

            await signInManager.SignInAsync(user, true, "");
        }

        private async Task ChangePassword(EditUserModel model)
        {
            await Task.Yield();
            throw new System.NotImplementedException();
        }
    }
}