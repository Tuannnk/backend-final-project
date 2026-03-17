using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("tai-khoan")]
    public class TaiKhoanViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang cá nhân";
            return View("~/Areas/Client/Views/TaiKhoan/Index.cshtml");
        }
    }
}

