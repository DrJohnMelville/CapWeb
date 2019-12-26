using System.ComponentModel.DataAnnotations;

namespace IdentityServer4.Quickstart.UI
{
    public class ForgotPasswordModel
    {
        [EmailAddress]
        public string EmailAddress { get; set; } = "";

    }
}