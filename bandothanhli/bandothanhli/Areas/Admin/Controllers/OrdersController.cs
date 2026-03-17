using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Quản lý đơn hàng";
            return View();
        }
    }
}

