using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý danh mục";
            return View();
        }
    }
}

