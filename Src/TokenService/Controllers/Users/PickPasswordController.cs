using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using TokenService.Models;

namespace TokenService.Controllers.Users
{
    

    public class PickPasswordController: Controller
    {
        private readonly UserManager<ApplicationUser> userManager;

        public PickPasswordController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Reset(string user, string token) =>
            View(new PickPasswordModel(user, token)
            {
                Title = "Reset your password",
                Explanation = "To reset your password, please type a new password into the two boxes below."
            });

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> Reset(PickPasswordModel model)
        {
            if (ModelState.IsValid && CheckPasswordsSame(model) && await LoadUser(model) is {} user)
            {
                return ModelState.CheckResult(
                    await userManager.ResetPasswordAsync(user, model.PermissionHash, model.Password)) ?
                    View("PasswordReset") : View(model);
            }
            return View(model);
        }

        private async Task<ApplicationUser?> LoadUser(PickPasswordModel model)
        {
            var user = await userManager.FindByNameAsync(model.User);
            if (user == null)
            {
                ModelState.AddModelError("User", "User not found");
            }
            return user;
        }

        private bool CheckPasswordsSame(PickPasswordModel model)
        {
            if (model.Password.Equals(model.PasswordVerification, StringComparison.Ordinal)) return true;
            ModelState.AddModelError("Password", "Password and Verification must be the same");
            return false;
        }

    }

    public class PickPasswordModel
    {
        public PickPasswordModel()
        {
        }

        public PickPasswordModel(string encodedUser, string encodedToken)
        {
            User = encodedUser;
            PermissionHash = encodedToken;
        }

        public string Explanation { get; set; } = "";
        public string Title { get; set; } = "";
        public string User { get; set; } = "";
        public string PermissionHash { get; set; } = "";
        public string Password { get; set; } = "";
        public string ButtonText { get; set; } = "Reset Password";
        [Compare("Password")]
        public string PasswordVerification { get; set; } = "";

    }
}