using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý người dùng";
            return View();
        }
    }
}

