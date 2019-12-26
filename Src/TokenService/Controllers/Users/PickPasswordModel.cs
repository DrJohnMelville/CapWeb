using System.ComponentModel.DataAnnotations;

namespace TokenService.Controllers.Users
{
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