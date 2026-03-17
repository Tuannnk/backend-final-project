using bandothanhli.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    public class SanPhamViewController : Controller
    {
        private readonly AppDbContext _db;

        public SanPhamViewController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /san-pham/chi-tiet/{id}
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var sanPham = await _db.SanPhams
                .Include(s => s.NguoiBan)
                .Include(s => s.DanhMuc)
                .Include(s => s.TinhTrang)
                .Include(s => s.AnhSanPhams)
                .Include(s => s.DanhGias)
                .ThenInclude(d => d.NguoiDanhGia)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanPham == null)
                return NotFound();

            return View(sanPham);
        }
    }
}
