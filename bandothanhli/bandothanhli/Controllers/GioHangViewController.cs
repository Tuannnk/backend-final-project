using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("gio-hang")]
    public class GioHangViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang giỏ hàng";
            return View("~/Views/GioHang/Index.cshtml");
        }
    }
}

