using Microsoft.AspNetCore.Mvc;

namespace bandothanhli.Areas.Client.Controllers
{
    [Area("Client")]
    [Route("san-pham")]
    public class SanPhamViewController : Controller
    {
        [HttpGet("{id:guid}")]
        public IActionResult ChiTiet(Guid id)
        {
            ViewData["ProductId"] = id;
            ViewData["Title"] = "Chi tiết sản phẩm";
            return View("~/Areas/Client/Views/SanPham/ChiTiet.cshtml");
        }
    }
}

