using bandothanhli.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/tinh-trang")]
    public class TinhTrangSanPhamController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TinhTrangSanPhamController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/tinh-trang
        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            var data = await _db.TinhTrangSanPhams
                .OrderByDescending(t => t.DiemTinhTrang)
                .Select(t => new { t.Id, t.TenTinhTrang, t.DiemTinhTrang, t.MoTa })
                .ToListAsync();

            return Ok(data);
        }
    }
}

