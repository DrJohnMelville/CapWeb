namespace TokenService.Controllers.Users
{
    public class PickPasswordController
    {
    }

    public class PickPasswordModel
    {
        public string Explanation { get; set; } = "";
        public string Title { get; set; } = "";
        public string PermissionHash { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordVerification = "";

    }
}