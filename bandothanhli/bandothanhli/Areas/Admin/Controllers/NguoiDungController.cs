using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class NguoiDungController : AdminBaseController
    {
        private readonly AppDbContext _db;
        public NguoiDungController(AppDbContext db) => _db = db;

        // GET /Admin/NguoiDung
        public async Task<IActionResult> Index(string tuKhoa = "", int trang = 1)
        {
            const int pageSize = 10;
            var q = _db.NguoiDungs
                .Where(u => u.VaiTro == "User")
                .AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
                q = q.Where(u => u.HoTen.Contains(tuKhoa)
                               || u.Email.Contains(tuKhoa)
                               || u.SoDienThoai.Contains(tuKhoa));

            var total = await q.CountAsync();
            var list = await q
                .OrderByDescending(u => u.NgayTao)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new NguoiDungVM
                {
                    Id = u.Id,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    SoDienThoai = u.SoDienThoai,
                    VaiTro = u.VaiTro,
                    DaXacThuc = u.DaXacThuc,
                    NgayTao = u.NgayTao,
                    TongSanPham = u.SanPhams.Count,
                    TongDonHang = u.DonHangs.Count
                }).ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TongSo = total;
            return View(list);
        }

        // GET /Admin/NguoiDung/ChiTiet/{id}
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var u = await _db.NguoiDungs
                .Include(x => x.DiaChis)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (u == null) return NotFound();

            var vm = new NguoiDungVM
            {
                Id = u.Id,
                HoTen = u.HoTen,
                Email = u.Email,
                SoDienThoai = u.SoDienThoai,
                VaiTro = u.VaiTro,
                DaXacThuc = u.DaXacThuc,
                NgayTao = u.NgayTao,
                TongSanPham = await _db.SanPhams.CountAsync(s => s.NguoiBanId == id),
                TongDonHang = await _db.DonHangs.CountAsync(d => d.NguoiMuaId == id)
            };

            ViewBag.DonHangs = await _db.DonHangs
                .Where(d => d.NguoiMuaId == id)
                .Include(d => d.ChiTietDonHangs)
                .OrderByDescending(d => d.NgayTao)
                .Take(5)
                .Select(d => new DonHangVM
                {
                    Id = d.Id,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai,
                    NgayTao = d.NgayTao,
                    SoSanPham = d.ChiTietDonHangs.Count
                }).ToListAsync();

            return View(vm);
        }

        // POST /Admin/NguoiDung/DoiTrangThai/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(Guid id)
        {
            var u = await _db.NguoiDungs.FindAsync(id);
            if (u == null) return NotFound();

            u.DaXacThuc = !u.DaXacThuc;
            await _db.SaveChangesAsync();

            TempData["Success"] = u.DaXacThuc
                ? $"Đã kích hoạt tài khoản {u.HoTen}"
                : $"Đã vô hiệu hoá tài khoản {u.HoTen}";
            return RedirectToAction("Index");
        }

        // POST /Admin/NguoiDung/Xoa/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var u = await _db.NguoiDungs.FindAsync(id);
            if (u == null) return NotFound();

            bool laAdmin = u.VaiTro == "Admin";
            if (laAdmin)
            {
                TempData["Error"] = "Không thể xóa tài khoản quản trị.";
                return RedirectToAction("Index");
            }

            bool conDon = await _db.DonHangs.AnyAsync(d =>
                d.NguoiMuaId == id &&
                (d.TrangThai == "ChoPhanHoi" ||
                 d.TrangThai == "DaXacNhan" ||
                 d.TrangThai == "DangGiao"));

            if (conDon)
            {
                TempData["Error"] = "Không thể xóa! Người dùng còn đơn hàng đang xử lý.";
                return RedirectToAction("Index");
            }

            bool coDuLieuLienQuan =
                await _db.SanPhams.AnyAsync(s => s.NguoiBanId == id) ||
                await _db.DonHangs.AnyAsync(d => d.NguoiMuaId == id) ||
                await _db.DiaChis.AnyAsync(dc => dc.NguoiDungId == id) ||
                await _db.DanhGias.AnyAsync(dg => dg.NguoiDanhGiaId == id) ||
                await _db.TinNhans.AnyAsync(t => t.NguoiGuiId == id || t.NguoiNhanId == id);

            if (coDuLieuLienQuan)
            {
                TempData["Error"] = "Không thể xóa! Người dùng vẫn còn dữ liệu liên quan.";
                return RedirectToAction("Index");
            }

            _db.NguoiDungs.Remove(u);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa tài khoản {u.HoTen}";
            return RedirectToAction("Index");
        }
    }
}
