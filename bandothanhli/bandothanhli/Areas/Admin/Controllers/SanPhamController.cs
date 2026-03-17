using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class SanPhamController : AdminBaseController
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public SanPhamController(AppDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        public async Task<IActionResult> Index(string tuKhoa = "", Guid? danhMucId = null,
            string trangThai = "", int trang = 1)
        {
            const int pageSize = 10;
            var q = _db.SanPhams
                .Include(s => s.NguoiBan)
                .Include(s => s.DanhMuc)
                .Include(s => s.TinhTrang)
                .Include(s => s.AnhSanPhams)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
                q = q.Where(s => s.TieuDe.Contains(tuKhoa)
                               || s.NguoiBan.HoTen.Contains(tuKhoa));
            if (danhMucId.HasValue)
                q = q.Where(s => s.DanhMucId == danhMucId);
            if (!string.IsNullOrEmpty(trangThai))
                q = q.Where(s => s.TrangThai == trangThai);

            var total = await q.CountAsync();
            var list = await q
                .OrderByDescending(s => s.NgayTao)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SanPhamVM
                {
                    Id = s.Id,
                    TieuDe = s.TieuDe,
                    Gia = s.Gia,
                    GiaGoc = s.GiaGoc,
                    TrangThai = s.TrangThai,
                    DiaDiem = s.DiaDiem,
                    NgayTao = s.NgayTao,
                    TenNguoiBan = s.NguoiBan.HoTen,
                    EmailNguoiBan = s.NguoiBan.Email,
                    TenDanhMuc = s.DanhMuc.TenDanhMuc,
                    TenTinhTrang = s.TinhTrang.TenTinhTrang,
                    AnhDaiDien = s.AnhSanPhams
                        .Where(a => a.LaAnhDaiDien)
                        .OrderBy(a => a.ThuTu)
                        .Select(a => a.DuongDanAnh)
                        .FirstOrDefault(),
                    SoDanhGia = s.DanhGias.Count,
                    DiemTrungBinh = s.DanhGias.Any()
                        ? s.DanhGias.Average(dg => (double)dg.DiemDanhGia) : 0
                }).ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DanhMucId = danhMucId;
            ViewBag.TrangThai = trangThai;
            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TongSo = total;
            ViewBag.DanhSachDanhMuc = await _db.DanhMucs.OrderBy(d => d.TenDanhMuc).ToListAsync();
            return View(list);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(Guid id, string trangThai)
        {
            var s = await _db.SanPhams.FindAsync(id);
            if (s == null) return NotFound();
            s.TrangThai = trangThai;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật trạng thái sản phẩm";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var s = await _db.SanPhams
                .Include(x => x.AnhSanPhams)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (s == null) return NotFound();

            bool conDon = await _db.ChiTietDonHangs
                .Include(ct => ct.DonHang)
                .AnyAsync(ct => ct.SanPhamId == id &&
                    (ct.DonHang.TrangThai == "ChoPhanHoi" ||
                     ct.DonHang.TrangThai == "DaXacNhan" ||
                     ct.DonHang.TrangThai == "DangGiao"));
            if (conDon)
            {
                TempData["Error"] = "Không thể xóa! Sản phẩm đang có đơn hàng xử lý.";
                return RedirectToAction("Index");
            }

            bool coLichSuDonHang = await _db.ChiTietDonHangs.AnyAsync(ct => ct.SanPhamId == id);
            if (coLichSuDonHang)
            {
                TempData["Error"] = "Không thể xóa! Sản phẩm đã phát sinh trong đơn hàng.";
                return RedirectToAction("Index");
            }

            bool coDuLieuLienQuan =
                await _db.DanhGias.AnyAsync(dg => dg.SanPhamId == id) ||
                await _db.TinNhans.AnyAsync(t => t.SanPhamId == id);

            if (coDuLieuLienQuan)
            {
                TempData["Error"] = "Không thể xóa! Sản phẩm vẫn còn đánh giá hoặc tin nhắn liên quan.";
                return RedirectToAction("Index");
            }

            foreach (var anh in s.AnhSanPhams)
            {
                var path = Path.Combine(_env.WebRootPath,
                    anh.DuongDanAnh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _db.SanPhams.Remove(s);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa sản phẩm '{s.TieuDe}'";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaAnh(Guid id)
        {
            var anh = await _db.AnhSanPhams.FindAsync(id);
            if (anh == null) return NotFound();

            var path = Path.Combine(_env.WebRootPath,
                anh.DuongDanAnh.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _db.AnhSanPhams.Remove(anh);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã xóa ảnh";
            return RedirectToAction("Index");
        }
    }
}
