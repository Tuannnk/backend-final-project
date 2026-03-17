using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("gio-hang")]
    public class GioHangViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang giỏ hàng";
            return View("~/Areas/Client/Views/GioHang/Index.cshtml");
        }
    }
}

