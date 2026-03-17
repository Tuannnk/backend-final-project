using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("don-hang")]
    public class DonHangViewController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect("/don-hang/mua");
        }

        [HttpGet("mua")]
        public IActionResult DonMua()
        {
            ViewData["Title"] = "Đơn mua";
            return View("~/Views/DonHang/Mua.cshtml");
        }

        [HttpGet("ban")]
        public IActionResult DonBan()
        {
            ViewData["Title"] = "Đơn bán";
            return View("~/Views/DonHang/Ban.cshtml");
        }

        [HttpGet("{id:guid}")]
        public IActionResult ChiTiet(Guid id)
        {
            ViewData["Title"] = "Chi tiết đơn hàng";
            ViewData["OrderId"] = id;
            return View("~/Views/DonHang/ChiTiet.cshtml");
        }
    }
}

