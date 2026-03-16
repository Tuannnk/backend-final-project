using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("auth")]
    public class AuthViewController : Controller
    {
        [HttpGet("dang-ky")]
        public IActionResult DangKy() => View("~/Views/Auth/DangKy.cshtml");

        [HttpGet("dang-nhap")]
        public IActionResult DangNhap() => View("~/Views/Auth/DangNhap.cshtml");

        [HttpGet("quen-mat-khau")]
        public IActionResult QuenMatKhau() => View("~/Views/Auth/QuenMatKhau.cshtml");
    }
}