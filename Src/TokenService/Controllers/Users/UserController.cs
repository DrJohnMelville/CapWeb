using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using TokenService.Data;
using TokenService.Models;

namespace TokenService.Controllers.Users
{
    public class UserController : Controller
    {
        private readonly IHttpContextAccessor contextFactory;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly Func<ApplicationDbContext> dbFactory;

        public UserController(IHttpContextAccessor contextFactory, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager, Func<ApplicationDbContext> dbFactory)
        {
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbFactory = dbFactory;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            return View(await AddSites(new EditUserModel(CurrentUserClaimPrincipal().Claims)));
        }

        private async Task<EditUserModel> AddSites(EditUserModel editUserModel)
        {
            var sub = CurrentUserClaimPrincipal().Claims.ClaimByName(JwtClaimTypes.Subject);
            editUserModel.Privileges = await 
                dbFactory().UserPrivileges.AsNoTracking().Where(i => i.UserId == sub)
                    .Select(i=>new WebsiteMembership(i.Site.FriendlyName, i.Privilege))
                    .ToListAsync();
            return editUserModel;
        }

        private ClaimsPrincipal CurrentUserClaimPrincipal() => contextFactory.HttpContext.User;

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

            model.CurrentPassword = "";
            return View(await AddSites(model));
        }
        private async Task ChangeName(EditUserModel model)
        {
            var user = await CurrentUserAsync();
            await AddOrReplaceClaim(model, user, new Claim(JwtClaimTypes.Name, model.FullName));
            await signInManager.SignInAsync(user, true, "");
        }

        private Task<ApplicationUser> CurrentUserAsync() => userManager.GetUserAsync(CurrentUserClaimPrincipal());

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
        }

        private async Task ChangePassword(EditUserModel model)
        {
            var user = await CurrentUserAsync();
            if (!CheckPasswordsSame(model)) return;
            HandlePasswordResetResponse(model, await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.Password));
        }

        private void HandlePasswordResetResponse(EditUserModel model, IdentityResult result)
        {
            if (ModelState.CheckResult(result))
            {
                PreventNewPasswordFromReturningToClient(model);
            }
        }

        private static void PreventNewPasswordFromReturningToClient(EditUserModel model) => 
            model.Password = model.PasswordVerification = "";

        private bool CheckPasswordsSame(EditUserModel model)
        {
            model.Password ??= "";
            model.PasswordVerification ??= "";
            if (model.Password.Equals(model.PasswordVerification, StringComparison.Ordinal)) return true;
            ModelState.AddModelError("Password", "Password and Verification must be the same");
            return false;
        }
    }

    public static class ModelStateDictionaryExtensions
    {
        public static bool CheckResult(this ModelStateDictionary dict, IdentityResult result)
        {
            if (result.Succeeded) return true;
            foreach (var error in result.Errors)
            {
                dict.AddModelError("", error.Description);
            }

            return false;
        }
    }
}