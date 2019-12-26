using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Quickstart.UI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokenService.Controllers.Users;
using TokenService.Data;
using TokenService.Models;

namespace TokenService.Controllers.Admin
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IPasswordResetNotificationSender emailSender;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, 
            IPasswordResetNotificationSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.emailSender = emailSender;
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
    }

    public class NewUserModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
        
        [Required]
        public string FullName { get; set; } = "";
    }
}