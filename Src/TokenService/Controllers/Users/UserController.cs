using Microsoft.AspNetCore.Mvc;

namespace TokenService.Controllers.Users
{
    public class UserController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}