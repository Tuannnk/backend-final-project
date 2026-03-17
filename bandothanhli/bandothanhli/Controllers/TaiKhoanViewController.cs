using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("tai-khoan")]
    public class TaiKhoanViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang cá nhân";
            return View("~/Views/TaiKhoan/Index.cshtml");
        }
    }
}

