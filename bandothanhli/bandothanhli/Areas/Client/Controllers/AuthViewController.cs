using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("auth")]
    public class AuthViewController : Controller
    {
        [HttpGet("dang-ky")]
        public IActionResult DangKy() => View("~/Areas/Client/Views/Auth/DangKy.cshtml");

        [HttpGet("dang-nhap")]
        public IActionResult DangNhap() => View("~/Areas/Client/Views/Auth/DangNhap.cshtml");

        [HttpGet("quen-mat-khau")]
        public IActionResult QuenMatKhau() => View("~/Areas/Client/Views/Auth/QuenMatKhau.cshtml");
    }
}

