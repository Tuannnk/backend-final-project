using bandothanhli.Data;
using bandothanhli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/danh-muc")]
    public class DanhMucController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DanhMucController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/danh-muc
        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            var data = await _db.DanhMucs
                .Where(d => d.DanhMucChaId == null)
                .Include(d => d.DanhMucCons)
                .Select(d => new
                {
                    d.Id,
                    d.TenDanhMuc,
                    d.Slug,
                    d.IconUrl,
                    d.ThuTu,
                    DanhMucCon = d.DanhMucCons.Select(c => new
                    {
                        c.Id,
                        c.TenDanhMuc,
                        c.Slug,
                        c.ThuTu
                    })
                })
                .OrderBy(d => d.ThuTu)
                .ToListAsync();

            return Ok(data);
        }

        // GET /api/danh-muc/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var danhMuc = await _db.DanhMucs
                .Include(d => d.DanhMucCons)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danhMuc == null) return NotFound(new { message = "Không tìm thấy danh mục" });
            return Ok(danhMuc);
        }

        // POST /api/danh-muc (Admin)
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Tao([FromBody] DanhMuc dto)
        {
            var danhMuc = new DanhMuc
            {
                Id = Guid.NewGuid(),
                TenDanhMuc = dto.TenDanhMuc,
                Slug = dto.Slug,
                IconUrl = dto.IconUrl,
                ThuTu = dto.ThuTu,
                DanhMucChaId = dto.DanhMucChaId
            };

            await _db.DanhMucs.AddAsync(danhMuc);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Tạo danh mục thành công", danhMuc });
        }

        // PUT /api/danh-muc/{id} (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CapNhat(Guid id, [FromBody] DanhMuc dto)
        {
            var danhMuc = await _db.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            danhMuc.TenDanhMuc = dto.TenDanhMuc ?? danhMuc.TenDanhMuc;
            danhMuc.Slug = dto.Slug ?? danhMuc.Slug;
            danhMuc.IconUrl = dto.IconUrl ?? danhMuc.IconUrl;
            danhMuc.ThuTu = dto.ThuTu;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // DELETE /api/danh-muc/{id} (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var danhMuc = await _db.DanhMucs.FindAsync(id);
            if (danhMuc == null) return NotFound(new { message = "Không tìm thấy danh mục" });

            var coSanPham = await _db.SanPhams.AnyAsync(s => s.DanhMucId == id);
            if (coSanPham) return BadRequest(new { message = "Danh mục đang có sản phẩm, không thể xóa" });

            _db.DanhMucs.Remove(danhMuc);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Xóa danh mục thành công" });
        }
    }
}