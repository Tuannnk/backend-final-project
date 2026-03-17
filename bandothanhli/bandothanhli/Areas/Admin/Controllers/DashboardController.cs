using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class DashboardController : AdminBaseController
    {
        private readonly AppDbContext _db;
        public DashboardController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var thangNay = new DateTime(today.Year, today.Month, 1);
            var thangTruoc = thangNay.AddMonths(-1);
            var startDate = today.AddDays(-29);

            var doanhThu30NgayRaw = await _db.ThanhToans
                .Where(t => t.TrangThai == "DaThanhToan"
                         && t.NgayThanhToan.HasValue
                         && t.NgayThanhToan.Value.Date >= startDate)
                .GroupBy(t => t.NgayThanhToan!.Value.Date)
                .Select(g => new
                {
                    Ngay = g.Key,
                    DoanhThu = g.Sum(x => x.SoTien),
                    SoDonHang = g.Count()
                })
                .ToListAsync();

            var doanhThuTheoNgay = doanhThu30NgayRaw.ToDictionary(x => x.Ngay);
            var bieuDoDoanhThu = Enumerable.Range(0, 30)
                .Select(offset =>
                {
                    var ngay = startDate.AddDays(offset);
                    doanhThuTheoNgay.TryGetValue(ngay, out var value);

                    return new DoanhThuTheoNgayVM
                    {
                        Ngay = ngay.ToString("dd/MM"),
                        DoanhThu = value?.DoanhThu ?? 0,
                        SoDonHang = value?.SoDonHang ?? 0
                    };
                })
                .ToList();

            var vm = new DashboardVM
            {
                // ── Thẻ thống kê nhanh ──
                TongNguoiDung = await _db.NguoiDungs.CountAsync(u => u.VaiTro == "User"),
                TongSanPham = await _db.SanPhams.CountAsync(),
                TongDonHang = await _db.DonHangs.CountAsync(),
                TinNhanChuaDoc = await _db.TinNhans.CountAsync(t => !t.DaDoc),

                DoanhThuHomNay = await _db.ThanhToans
                    .Where(t => t.TrangThai == "DaThanhToan"
                             && t.NgayThanhToan.HasValue
                             && t.NgayThanhToan.Value.Date == today)
                    .SumAsync(t => (decimal?)t.SoTien) ?? 0,

                DoanhThuThangNay = await _db.ThanhToans
                    .Where(t => t.TrangThai == "DaThanhToan"
                             && t.NgayThanhToan >= thangNay)
                    .SumAsync(t => (decimal?)t.SoTien) ?? 0,

                DoanhThuThangTruoc = await _db.ThanhToans
                    .Where(t => t.TrangThai == "DaThanhToan"
                             && t.NgayThanhToan >= thangTruoc
                             && t.NgayThanhToan < thangNay)
                    .SumAsync(t => (decimal?)t.SoTien) ?? 0,

                // ── Đơn hàng theo trạng thái ──
                DonChoPhanHoi = await _db.DonHangs.CountAsync(d => d.TrangThai == "ChoPhanHoi"),
                DonDaXacNhan = await _db.DonHangs.CountAsync(d => d.TrangThai == "DaXacNhan"),
                DonDangGiao = await _db.DonHangs.CountAsync(d => d.TrangThai == "DangGiao"),
                DonDaGiao = await _db.DonHangs.CountAsync(d => d.TrangThai == "DaGiao"),
                DonHuyDon = await _db.DonHangs.CountAsync(d => d.TrangThai == "HuyDon"),

                // ── Sản phẩm ──
                SanPhamDangBan = await _db.SanPhams.CountAsync(s => s.TrangThai == "DangBan"),
                SanPhamDaBan = await _db.SanPhams.CountAsync(s => s.TrangThai == "DaBan"),
                SanPhamAnHang = await _db.SanPhams.CountAsync(s => s.TrangThai == "AnHang"),

                // ── Người dùng ──
                NguoiDungMoiThangNay = await _db.NguoiDungs
                    .CountAsync(u => u.VaiTro == "User" && u.NgayTao >= thangNay),
                NguoiDungChuaXacThuc = await _db.NguoiDungs
                    .CountAsync(u => u.VaiTro == "User" && !u.DaXacThuc),

                // ── Đơn hàng gần đây ──
                DonHangGanDay = await _db.DonHangs
                    .Include(d => d.NguoiMua)
                    .Include(d => d.ChiTietDonHangs)
                    .Include(d => d.ThanhToan)
                    .OrderByDescending(d => d.NgayTao)
                    .Take(8)
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
                    }).ToListAsync(),

                // ── Sản phẩm mới ──
                SanPhamGanDay = await _db.SanPhams
                    .Include(s => s.NguoiBan)
                    .Include(s => s.DanhMuc)
                    .Include(s => s.TinhTrang)
                    .Include(s => s.AnhSanPhams)
                    .OrderByDescending(s => s.NgayTao)
                    .Take(6)
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
                    }).ToListAsync(),

                // ── Biểu đồ doanh thu 30 ngày ──
                BieuDoDoanhThu = bieuDoDoanhThu,

                // ── Sản phẩm theo loại ──
                SanPhamTheoLoai = await _db.DanhMucs
                    .Where(d => d.SanPhams.Any())
                    .Select(d => new SanPhamTheoLoaiVM
                    {
                        TenDanhMuc = d.TenDanhMuc,
                        SoLuong = d.SanPhams.Count
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
