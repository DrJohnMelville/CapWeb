using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using SendMailService;
using TokenService.Models;

namespace IdentityServer4.Quickstart.UI
{
    public interface IPasswordResetNotificationSender
    {
        Task SendPasswordResetEmail(ApplicationUser user, string subject,
            Func<string, string, string> bodyText);
    }
    public class PasswordResetNotificationSender: IPasswordResetNotificationSender
    {
        private readonly HttpRequest request;
        private readonly ISendEmailService emailSender;
        private readonly UserManager<ApplicationUser> userManager;
        public PasswordResetNotificationSender(IHttpContextAccessor context, UserManager<ApplicationUser> userManager, 
            ISendEmailService emailSender)
        {
            this.userManager = userManager;
            this.emailSender = emailSender;
            request = context.HttpContext.Request;
        }

        
        public async Task SendPasswordResetEmail(ApplicationUser user, string subject,
            Func<string, string, string> bodyText)
        {
            if (user != null && await EmailMessageForUser(user, bodyText) is {} resetMessage)
            {
                await emailSender.SendEmail(user.UserName, subject,
                    resetMessage);
            }
        }

        private async Task<string> EmailMessageForUser(ApplicationUser user, 
            Func<string, string, string> createMessage)
        {
            return createMessage(user.UserName,
                ResetTokenAsHtmlParagraphString(PasswordResetUrl(user.UserName, 
                    await userManager.GeneratePasswordResetTokenAsync(user))));
        }

        private static string ResetTokenAsHtmlParagraphString(string resrtUrl)
        {
            return $"<p><a href='{resrtUrl}'>{HttpUtility.HtmlEncode(resrtUrl)}</a></p> ";
        }

        private string PasswordResetUrl(string email, string resetToken) =>
            $"{WebsiteRootUrl()}/PickPassword/Reset?user={HttpUtility.UrlEncode(email)}" +
            $"&token={HttpUtility.UrlEncode(resetToken)}";

        private string WebsiteRootUrl() =>
            $"{request.Scheme}://{request.Host}{this.request.PathBase}";
    }
}