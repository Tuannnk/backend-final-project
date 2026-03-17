using bandothanhli.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/admin/thong-ke")]
    [Authorize(Roles = "admin")]
    public class AdminThongKeController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminThongKeController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/admin/thong-ke/tong-quan?nam=2026&thang=3
        [HttpGet("tong-quan")]
        public async Task<IActionResult> TongQuan([FromQuery] int? nam, [FromQuery] int? thang)
        {
            var now = DateTime.Now;
            var year = nam ?? now.Year;
            var month = thang ?? now.Month;

            if (month < 1 || month > 12)
                return BadRequest(new { message = "Tháng không hợp lệ" });

            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var tongNguoiDung = await _db.NguoiDungs.CountAsync();
            var tongDonHang = await _db.DonHangs.CountAsync();

            var nguoiDungThangNay = await _db.NguoiDungs.CountAsync(n => n.NgayTao >= start && n.NgayTao < end);
            var donHangThangNay = await _db.DonHangs.CountAsync(d => d.NgayTao >= start && d.NgayTao < end);

            var doanhThuThangNay = await _db.DonHangs
                .Where(d => d.NgayTao >= start && d.NgayTao < end && d.TrangThai != "da_huy")
                .SumAsync(d => (decimal?)d.TongTien) ?? 0;

            var topDanhMuc = await _db.ChiTietDonHangs
                .Where(c => c.DonHang.NgayTao >= start && c.DonHang.NgayTao < end && c.DonHang.TrangThai != "da_huy")
                .GroupBy(c => new { c.SanPham.DanhMucId, c.SanPham.DanhMuc.TenDanhMuc })
                .Select(g => new
                {
                    danhMucId = g.Key.DanhMucId,
                    tenDanhMuc = g.Key.TenDanhMuc,
                    soLuongBan = g.Sum(x => x.SoLuong),
                    doanhThu = g.Sum(x => x.GiaTaiThoiDiem * x.SoLuong)
                })
                .OrderByDescending(x => x.soLuongBan)
                .ThenByDescending(x => x.doanhThu)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                nam = year,
                thang = month,
                tongNguoiDung,
                tongDonHang,
                nguoiDungThangNay,
                donHangThangNay,
                doanhThuThangNay,
                topDanhMuc
            });
        }
    }
}

