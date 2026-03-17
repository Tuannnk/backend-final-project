using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("dang-tin")]
    public class DangTinViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang đăng tin";
            return View("~/Views/SanPham/DangTin.cshtml");
        }
    }
}

