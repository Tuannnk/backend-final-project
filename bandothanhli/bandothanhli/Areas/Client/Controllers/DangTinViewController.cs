using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("dang-tin")]
    public class DangTinViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang đăng tin";
            return View("~/Areas/Client/Views/SanPham/DangTin.cshtml");
        }
    }
}

