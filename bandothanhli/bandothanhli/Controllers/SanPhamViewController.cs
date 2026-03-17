using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Controllers
{
    [Route("san-pham")]
    public class SanPhamViewController : Controller
    {
        [HttpGet("{id:guid}")]
        public IActionResult ChiTiet(Guid id)
        {
            ViewData["ProductId"] = id;
            ViewData["Title"] = "Chi tiết sản phẩm";
            return View("~/Views/SanPham/ChiTiet.cshtml");
        }
    }
}

