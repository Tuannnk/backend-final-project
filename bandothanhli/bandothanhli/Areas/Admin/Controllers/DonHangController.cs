using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;
using bandothanhli.Models;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class DonHangController : AdminBaseController
    {
        private readonly AppDbContext _db;
        public DonHangController(AppDbContext db) => _db = db;

        // ── DANH SÁCH ──────────────────────────────────────────
        public async Task<IActionResult> Index(string trangThai = "", int trang = 1)
        {
            const int pageSize = 10;
            var q = _db.DonHangs
                .Include(d => d.NguoiMua)
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.ThanhToan)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                q = q.Where(d => d.TrangThai == trangThai);

            var total = await q.CountAsync();
            var list = await q
                .OrderByDescending(d => d.NgayTao)
                .Skip((trang - 1) * pageSize)
                .Take(pageSize)
                .Select(d => new DonHangVM
                {
                    Id = d.Id,
                    TongTien = d.TongTien,
                    TrangThai = d.TrangThai,
                    GhiChu = d.GhiChu,
                    NgayTao = d.NgayTao,
                    TenNguoiMua = d.NguoiMua.HoTen,
                    EmailNguoiMua = d.NguoiMua.Email,
                    SoSanPham = d.ChiTietDonHangs.Count,
                    TrangThaiThanhToan = d.ThanhToan != null
                        ? d.ThanhToan.TrangThai : "ChuaThanhToan"
                }).ToListAsync();

            ViewBag.TrangThai = trangThai;
            ViewBag.TrangHienTai = trang;
            ViewBag.TongTrang = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.TongSo = total;
            return View(list);
        }

        // ── CHI TIẾT ──────────────────────────────────────────
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var dh = await _db.DonHangs
                .Include(d => d.NguoiMua)
                .Include(d => d.DiaChi)
                .Include(d => d.ThanhToan)
                .Include(d => d.TheoDoidonHangs.OrderByDescending(t => t.NgayCapNhat))
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(ct => ct.SanPham)
                        .ThenInclude(s => s.AnhSanPhams)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dh == null) return NotFound();

            var vm = new DonHangChiTietVM
            {
                Id = dh.Id,
                TongTien = dh.TongTien,
                TrangThai = dh.TrangThai,
                GhiChu = dh.GhiChu,
                NgayTao = dh.NgayTao,
                NgayCapNhat = dh.NgayCapNhat,
                NguoiMuaId = dh.NguoiMuaId,
                TenNguoiMua = dh.NguoiMua.HoTen,
                EmailNguoiMua = dh.NguoiMua.Email,
                SdtNguoiMua = dh.NguoiMua.SoDienThoai,
                DiaChiGiao = $"{dh.DiaChi.DuongPho}, {dh.DiaChi.QuanHuyen}, {dh.DiaChi.TinhThanh}",

                ChiTiet = dh.ChiTietDonHangs.Select(ct => new ChiTietDonHangVM
                {
                    SanPhamId = ct.SanPhamId,
                    TieuDe = ct.SanPham.TieuDe,
                    GiaTaiThoiDiem = ct.GiaTaiThoiDiem,
                    SoLuong = ct.SoLuong,
                    AnhDaiDien = ct.SanPham.AnhSanPhams
                        .Where(a => a.LaAnhDaiDien)
                        .OrderBy(a => a.ThuTu)
                        .Select(a => a.DuongDanAnh)
                        .FirstOrDefault()
                }).ToList(),

                ThanhToan = dh.ThanhToan == null ? null : new ThanhToanVM
                {
                    Id = dh.ThanhToan.Id,
                    PhuongThuc = dh.ThanhToan.PhuongThuc,
                    SoTien = dh.ThanhToan.SoTien,
                    TrangThai = dh.ThanhToan.TrangThai,
                    MaGiaoDich = dh.ThanhToan.MaGiaoDich,
                    NgayThanhToan = dh.ThanhToan.NgayThanhToan
                },

                LichSuTheoDoi = dh.TheoDoidonHangs.Select(t => new TheDoiVM
                {
                    Id = t.Id,
                    TrangThai = t.TrangThai,
                    GhiChu = t.GhiChu,
                    NgayCapNhat = t.NgayCapNhat
                }).ToList()
            };

            ViewBag.ThemTheDoiVM = new ThemTheDoiVM { DonHangId = id };
            return View(vm);
        }

        // ── THÊM TRẠNG THÁI THEO DÕI ──────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemTheoDoi(ThemTheDoiVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng chọn trạng thái";
                return RedirectToAction("ChiTiet", new { id = model.DonHangId });
            }

            var dh = await _db.DonHangs.FindAsync(model.DonHangId);
            if (dh == null) return NotFound();

            _db.TheoDoidonHangs.Add(new TheoDoidonHang
            {
                Id = Guid.NewGuid(),
                DonHangId = model.DonHangId,
                TrangThai = model.TrangThai,
                GhiChu = model.GhiChu,
                NgayCapNhat = DateTime.Now
            });

            dh.TrangThai = model.TrangThai;
            dh.NgayCapNhat = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái: {model.TrangThai}";
            return RedirectToAction("ChiTiet", new { id = model.DonHangId });
        }

        // ── XÁC NHẬN THANH TOÁN ───────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(XacNhanThanhToanVM model)
        {
            var tt = await _db.ThanhToans
                .Include(t => t.DonHang)
                .FirstOrDefaultAsync(t => t.DonHangId == model.DonHangId);
            if (tt == null) return NotFound();

            if (tt.TrangThai == "DaThanhToan")
            {
                TempData["Error"] = "Đơn hàng này đã được xác nhận thanh toán rồi.";
                return RedirectToAction("ChiTiet", new { id = model.DonHangId });
            }

            tt.TrangThai = "DaThanhToan";
            tt.NgayThanhToan = DateTime.Now;
            tt.MaGiaoDich = string.IsNullOrEmpty(model.MaGiaoDich)
                ? $"ADMIN-{DateTime.Now:yyyyMMddHHmmss}"
                : model.MaGiaoDich;

            if (tt.DonHang.TrangThai == "ChoPhanHoi")
            {
                tt.DonHang.TrangThai = "DaXacNhan";
                tt.DonHang.NgayCapNhat = DateTime.Now;

                _db.TheoDoidonHangs.Add(new TheoDoidonHang
                {
                    Id = Guid.NewGuid(),
                    DonHangId = model.DonHangId,
                    TrangThai = "DaXacNhan",
                    GhiChu = "Đã xác nhận thanh toán",
                    NgayCapNhat = DateTime.Now
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Xác nhận thanh toán thành công!";
            return RedirectToAction("ChiTiet", new { id = model.DonHangId });
        }
    }
}