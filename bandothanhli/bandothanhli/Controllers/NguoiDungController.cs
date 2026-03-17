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
    [Route("api/nguoi-dung")]
    [Authorize]
    public class NguoiDungController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NguoiDungController(AppDbContext db)
        {
            _db = db;
        }

        // Lấy id từ token
        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET /api/nguoi-dung/ho-so
        [HttpGet("ho-so")]
        public async Task<IActionResult> HoSo()
        {
            var id = GetCurrentUserId();
            var nguoiDung = await _db.NguoiDungs
                .Where(n => n.Id == id)
                .Select(n => new
                {
                    n.Id,
                    n.HoTen,
                    n.Email,
                    n.SoDienThoai,
                    n.VaiTro,
                    n.DaXacThuc,
                    n.NgayTao
                })
                .FirstOrDefaultAsync();

            if (nguoiDung == null) return NotFound();
            return Ok(nguoiDung);
        }

        // PUT /api/nguoi-dung/cap-nhat
        [HttpPut("cap-nhat")]
        public async Task<IActionResult> CapNhat([FromBody] CapNhatNguoiDungDTO dto)
        {
            var id = GetCurrentUserId();
            var nguoiDung = await _db.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound();

            nguoiDung.HoTen = dto.HoTen ?? nguoiDung.HoTen;
            nguoiDung.SoDienThoai = dto.SoDienThoai ?? nguoiDung.SoDienThoai;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công" });
        }

        // GET /api/nguoi-dung (Admin)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DanhSach([FromQuery] int trang = 1, [FromQuery] int soLuong = 10)
        {
            var query = _db.NguoiDungs.AsQueryable();
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(n => n.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(n => new
                {
                    n.Id,
                    n.HoTen,
                    n.Email,
                    n.SoDienThoai,
                    n.VaiTro,
                    n.DaXacThuc,
                    n.NgayTao
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }

        // GET /api/nguoi-dung/{id} (Admin)
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var nguoiDung = await _db.NguoiDungs
                .Where(n => n.Id == id)
                .Select(n => new
                {
                    n.Id,
                    n.HoTen,
                    n.Email,
                    n.SoDienThoai,
                    n.VaiTro,
                    n.DaXacThuc,
                    n.NgayTao
                })
                .FirstOrDefaultAsync();

            if (nguoiDung == null) return NotFound(new { message = "Không tìm thấy người dùng" });
            return Ok(nguoiDung);
        }

        // DELETE /api/nguoi-dung/{id} (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var nguoiDung = await _db.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            _db.NguoiDungs.Remove(nguoiDung);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Xóa thành công" });
        }

        // PUT /api/nguoi-dung/{id}/khoa (Admin)
        [HttpPut("{id}/khoa")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> KhoaTaiKhoan(Guid id)
        {
            var nguoiDung = await _db.NguoiDungs.FindAsync(id);
            if (nguoiDung == null) return NotFound(new { message = "Không tìm thấy người dùng" });

            nguoiDung.DaXacThuc = !nguoiDung.DaXacThuc;
            await _db.SaveChangesAsync();

            var trangThai = nguoiDung.DaXacThuc ? "mở khóa" : "khóa";
            return Ok(new { message = $"Đã {trangThai} tài khoản thành công" });
        }
    }
}