using bandothanhli.Data;
using bandothanhli.DTOs;
using bandothanhli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/danh-gia")]
    public class DanhGiaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DanhGiaController(AppDbContext db)
        {
            _db = db;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET /api/danh-gia/san-pham/{sanPhamId}
        [HttpGet("san-pham/{sanPhamId}")]
        public async Task<IActionResult> DanhGiaSanPham(Guid sanPhamId, [FromQuery] int trang = 1, [FromQuery] int soLuong = 10)
        {
            var query = _db.DanhGias
                .Where(d => d.SanPhamId == sanPhamId)
                .Include(d => d.NguoiDanhGia);

            var total = await query.CountAsync();
            var diemTrungBinh = await query.AverageAsync(d => (double?)d.DiemDanhGia) ?? 0;

            var data = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(d => new
                {
                    d.Id,
                    d.DiemDanhGia,
                    d.BinhLuan,
                    d.NgayTao,
                    NguoiDanhGia = d.NguoiDanhGia.HoTen
                })
                .ToListAsync();

            return Ok(new { total, diemTrungBinh = Math.Round(diemTrungBinh, 1), trang, soLuong, data });
        }

        // POST /api/danh-gia/san-pham/{sanPhamId}
        [HttpPost("san-pham/{sanPhamId}")]
        [Authorize]
        public async Task<IActionResult> VietDanhGia(Guid sanPhamId, [FromBody] VietDanhGiaDTO dto)
        {
            var userId = GetCurrentUserId();

            // Kiểm tra đã mua sản phẩm chưa
            var daMua = await _db.DonHangs
                .Where(d => d.NguoiMuaId == userId && d.TrangThai == "hoan_thanh")
                .AnyAsync(d => d.ChiTietDonHangs.Any(c => c.SanPhamId == sanPhamId));

            if (!daMua)
                return BadRequest(new { message = "Bạn cần mua sản phẩm này trước khi đánh giá" });

            // Kiểm tra đã đánh giá chưa
            var daConh = await _db.DanhGias.AnyAsync(d => d.SanPhamId == sanPhamId && d.NguoiDanhGiaId == userId);
            if (daConh)
                return BadRequest(new { message = "Bạn đã đánh giá sản phẩm này rồi" });

            if (dto.DiemDanhGia < 1 || dto.DiemDanhGia > 5)
                return BadRequest(new { message = "Điểm đánh giá phải từ 1 đến 5" });

            var danhGia = new DanhGia
            {
                Id = Guid.NewGuid(),
                SanPhamId = sanPhamId,
                NguoiDanhGiaId = userId,
                DiemDanhGia = dto.DiemDanhGia,
                BinhLuan = dto.BinhLuan,
                NgayTao = DateTime.Now
            };

            await _db.DanhGias.AddAsync(danhGia);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đánh giá thành công" });
        }

        // PUT /api/danh-gia/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> SuaDanhGia(Guid id, [FromBody] VietDanhGiaDTO dto)
        {
            var userId = GetCurrentUserId();
            var danhGia = await _db.DanhGias.FindAsync(id);

            if (danhGia == null) return NotFound(new { message = "Không tìm thấy đánh giá" });
            if (danhGia.NguoiDanhGiaId != userId) return Forbid();

            danhGia.DiemDanhGia = dto.DiemDanhGia;
            danhGia.BinhLuan = dto.BinhLuan;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật đánh giá thành công" });
        }

        // DELETE /api/danh-gia/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> XoaDanhGia(Guid id)
        {
            var userId = GetCurrentUserId();
            var vaiTro = User.FindFirstValue(ClaimTypes.Role);
            var danhGia = await _db.DanhGias.FindAsync(id);

            if (danhGia == null) return NotFound(new { message = "Không tìm thấy đánh giá" });
            if (danhGia.NguoiDanhGiaId != userId && vaiTro != "admin") return Forbid();

            _db.DanhGias.Remove(danhGia);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Xóa đánh giá thành công" });
        }

        // GET /api/danh-gia/top-san-pham
        [HttpGet("top-san-pham")]
        public async Task<IActionResult> TopSanPham([FromQuery] int soLuong = 8)
        {
            if (soLuong < 1) soLuong = 1;
            if (soLuong > 20) soLuong = 20;

            var data = await _db.SanPhams
                .Where(s => s.TrangThai == "dang_ban" && s.DanhGias.Any())
                .Select(s => new
                {
                    s.Id,
                    s.TieuDe,
                    s.Gia,
                    s.DiaDiem,
                    DiemDanhGia = s.DanhGias.Average(d => (double)d.DiemDanhGia),
                    SoDanhGia = s.DanhGias.Count,
                    AnhDaiDien = s.AnhSanPhams
                        .Where(a => a.LaAnhDaiDien)
                        .Select(a => a.DuongDanAnh)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.DiemDanhGia)
                .ThenByDescending(x => x.SoDanhGia)
                .Take(soLuong)
                .ToListAsync();

            return Ok(data);
        }
    }
}
