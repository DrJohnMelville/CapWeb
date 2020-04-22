using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokenService.Controllers.Users;
using TokenService.Data;
using TokenService.Data.UserPriviliges;
using TokenService.Models;

namespace TokenService.Controllers.Admin
{
    [SecurityHeaders]
    [Authorize(Policy = "Administrator")]
    [AutoValidateAntiforgeryToken]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPasswordResetNotificationSender emailSender;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, 
            IPasswordResetNotificationSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.signInManager = signInManager;
        }

        // GET
        public async Task<IActionResult> Index() => 
            View(new AdminModel{Users = await db.Users.AsNoTracking().ToListAsync()});

        [HttpGet]
        public IActionResult NewUser() =>
            View(new NewUserModel());

        [HttpPost]
        public async Task<IActionResult> NewUser(NewUserModel userRequest)
        {
            if (!ModelState.IsValid) return View(userRequest);

            var user = new ApplicationUser(){UserName = userRequest.Email};
            var creatiionResult = await userManager.CreateAsync(user);
            if (!ModelState.CheckResult(creatiionResult)) return View(userRequest);
            var claimResult = await userManager.AddClaimsAsync(user, new[]
            {
                new Claim(JwtClaimTypes.Name, userRequest.FullName),
                new Claim(JwtClaimTypes.Email, userRequest.Email)
            });
            if (!ModelState.CheckResult(creatiionResult)) return View((userRequest));
            await emailSender.SendPasswordResetEmail(user, "Welcome to CapWeb (OBCAP and EWD)", CreateWelcomeMessage);
            return Redirect("/Admin");
        }
        
        private string CreateWelcomeMessage(string email, string resetTokenAsHtmlParagraphString)
        {
            return $"<p>An account on CapWeb has been created for user {email}.  To claim your account, please " +
                   $"click the link below and pick a password to access the site.</p> " +
                   resetTokenAsHtmlParagraphString +
                   "If you did not request this account,please send me an email at " +
                   "<a href='mailto:johnmelville@gmail.com'>John Melville.</a>  I do not intend to offer CapWeb " +
                   "accounts to people who do not want them.</p>";
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            var claims = await userManager.GetClaimsAsync(user);
            return View(await UserToModel(claims, id));
        }

        private async Task<EditUserModel> UserToModel(IList<Claim> claims, string id)
        {
            var sites = await db.ClientSites.AsNoTracking()
                .OrderBy(i => i.FriendlyName)
                .Select(i => new WebsiteInformation
                {
                    DisplayName = i.FriendlyName,
                    PrivateName = i.ShortName,
                    Privilege = SitePrivilege.None
                })
                .ToArrayAsync();
            var priv = await db.UserPrivileges.Where(i => i.UserId == id).ToListAsync();
            sites.Join<WebsiteInformation, UserPrivilege, string, int>(priv, i => i.PrivateName, i => i.SiteId, AssignPriv).ToList();
            return new EditUserModel
            {
                Id = id,
                FullName = GetClaim(claims, JwtClaimTypes.Name),
                Email = GetClaim(claims, JwtClaimTypes.Email),
                Sites = sites
            };
        }

        private int AssignPriv(WebsiteInformation arg1, UserPrivilege arg2)
        {
            arg1.Privilege = arg2.Privilege;
            return 1;
        }

        private string GetClaim(IList<Claim> claims, string claimType) => 
            claims.FirstOrDefault(i=>i.Type == claimType)?.Value??"";

        [HttpPost]
        public Task<IActionResult> EditUser(EditUserModel model, string button)
        {
            switch (button)
            {
                case "Update": return UpdateUser(model);
                case "Delete": return Task.FromResult((IActionResult)View("DeleteConfirm", model));
                case "NoDelete": return Task.FromResult((IActionResult)View(model));
                case "YesDelete": return DeleteUser(model);
                case "Impersonate": return ImpersonateUser(model);
                case "PasswordReset": return TryPasswordReset(model);
            }
            throw new InvalidOperationException("Invalid Button");
        }

        private async Task<IActionResult> TryPasswordReset(EditUserModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                await HandlePasswordReset(model);
            }
            return View(model);
        }

        private async Task HandlePasswordReset(EditUserModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, model.NewPassword);
            model.NewPassword = "";
        }

        private async Task<IActionResult> ImpersonateUser(EditUserModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);
            await signInManager.SignOutAsync();
            await signInManager.SignInAsync(user, false);
            return Redirect("/");
        }

        private async Task<IActionResult> DeleteUser(EditUserModel model)
        {
            await userManager.DeleteAsync(await userManager.FindByIdAsync(model.Id));
            return Redirect("/");
        }

        private async Task<IActionResult> UpdateUser(EditUserModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await userManager.FindByIdAsync(model.Id);
            await UpdateIdentityClaims(model, user);

            var websiteClaims = await db.UserPrivileges.Where(i => i.UserId == model.Id).ToListAsync();
            foreach (var claim in websiteClaims)
            {
                db.UserPrivileges.Remove(claim);
            }

            foreach (var newClaim in model.Sites.Where(i =>
                i.Privilege == SitePrivilege.Administrator || i.Privilege == SitePrivilege.User))
            {
                db.UserPrivileges.Add(new UserPrivilege()
                    {SiteId = newClaim.PrivateName, UserId = model.Id, Privilege = newClaim.Privilege});
            }

            await db.SaveChangesAsync();
            return View(model);
        }

        private async Task UpdateIdentityClaims(EditUserModel model, ApplicationUser user)
        {
            var claims = await userManager.GetClaimsAsync(user);
            await AddOrReplaceClaim(claims, JwtClaimTypes.Name, model.FullName, user);
            await AddOrReplaceClaim(claims, JwtClaimTypes.Email, model.Email, user);
            user.Email = model.Email;
            user.UserName = model.Email;
            user.NormalizedUserName = model.Email.ToUpper();
            await userManager.UpdateAsync(user);
        }

        private ValueTask AddOrReplaceClaim(IList<Claim> claims, string claimType, string value, 
            ApplicationUser user)
        {
            var oldClaim = claims.FirstOrDefault(i => i.Type.Equals(claimType, StringComparison.Ordinal));
            if (oldClaim == null) return new ValueTask(userManager.AddClaimAsync(user, new Claim(claimType, value)));
            if (oldClaim.Value.Equals(value, StringComparison.Ordinal)) return new ValueTask();
            return new ValueTask(userManager.ReplaceClaimAsync(user, oldClaim, new Claim(claimType, value)));
        }
    }

    public class EditUserModel:NewUserModel
    {
        public string Id { get; set; } = "";
    }
    public class NewUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        public string FullName { get; set; } = "";
        public string? NewPassword { get; set; }

        public WebsiteInformation[] Sites { get; set; } = Array.Empty<WebsiteInformation>();
    }

    public class WebsiteInformation
    {
        public String DisplayName { get; set; } = "";
        public String PrivateName { get; set; } = "";
        public SitePrivilege Privilege { get; set; }
    }
}