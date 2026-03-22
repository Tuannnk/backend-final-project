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
    [Route("api/don-hang")]
    [Authorize]
    public class DonHangController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DonHangController(AppDbContext db)
        {
            _db = db;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // POST /api/don-hang
        [HttpPost]
        public async Task<IActionResult> TaoDonHang([FromBody] TaoDonHangDTO dto)
        {
            var userId = GetCurrentUserId();

<<<<<<< HEAD
=======
            if (!string.Equals(dto.PhuongThucThanhToan, "thanh_toan_khi_nhan_hang", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Hiện chỉ hỗ trợ thanh toán khi nhận hàng" });

>>>>>>> origin/Tuannnk
            // Kiểm tra địa chỉ
            var diaChi = await _db.DiaChis.FirstOrDefaultAsync(d => d.Id == dto.DiaChiId && d.NguoiDungId == userId);
            if (diaChi == null) return BadRequest(new { message = "Địa chỉ không hợp lệ" });

            // Tính tổng tiền và kiểm tra sản phẩm
            decimal tongTien = 0;
            var chiTietList = new List<ChiTietDonHang>();

            foreach (var item in dto.SanPhams)
            {
                var sanPham = await _db.SanPhams.FindAsync(item.SanPhamId);
                if (sanPham == null || sanPham.TrangThai != "dang_ban")
                    return BadRequest(new { message = $"Sản phẩm không tồn tại hoặc đã ngừng bán" });

                if (sanPham.SoLuong < item.SoLuong)
                    return BadRequest(new { message = $"Sản phẩm '{sanPham.TieuDe}' không đủ số lượng" });

                tongTien += sanPham.Gia * item.SoLuong;
                chiTietList.Add(new ChiTietDonHang
                {
                    Id = Guid.NewGuid(),
                    SanPhamId = item.SanPhamId,
                    GiaTaiThoiDiem = sanPham.Gia,
                    SoLuong = item.SoLuong
                });

                // Trừ số lượng tồn kho
                sanPham.SoLuong -= item.SoLuong;
                if (sanPham.SoLuong == 0) sanPham.TrangThai = "da_ban";
            }

            var donHang = new DonHang
            {
                Id = Guid.NewGuid(),
                NguoiMuaId = userId,
                DiaChiId = dto.DiaChiId,
                TongTien = tongTien,
                TrangThai = "cho_xac_nhan",
                GhiChu = dto.GhiChu,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now,
                ChiTietDonHangs = chiTietList
            };

            // Tạo thanh toán
            var thanhToan = new ThanhToan
            {
                Id = Guid.NewGuid(),
                DonHangId = donHang.Id,
                PhuongThuc = dto.PhuongThucThanhToan,
                SoTien = tongTien,
                TrangThai = "cho_thanh_toan",
<<<<<<< HEAD
                MaGiaoDich = null
=======
                MaGiaoDich = $"COD-{Guid.NewGuid():N}"
>>>>>>> origin/Tuannnk
            };

            // Tạo theo dõi đơn hàng
            var theoDoi = new TheoDoidonHang
            {
                Id = Guid.NewGuid(),
                DonHangId = donHang.Id,
                TrangThai = "cho_xac_nhan",
                GhiChu = "Đơn hàng vừa được tạo",
                NgayCapNhat = DateTime.Now
            };

            await _db.DonHangs.AddAsync(donHang);
            await _db.ThanhToans.AddAsync(thanhToan);
            await _db.TheoDoidonHangs.AddAsync(theoDoi);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đặt hàng thành công", donHangId = donHang.Id, tongTien });
        }

        // GET /api/don-hang/cua-toi
        [HttpGet("cua-toi")]
        public async Task<IActionResult> DonHangCuaToi([FromQuery] int trang = 1, [FromQuery] int soLuong = 10)
        {
            var userId = GetCurrentUserId();
            var query = _db.DonHangs
                .Where(d => d.NguoiMuaId == userId)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                        .ThenInclude(s => s.AnhSanPhams);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(d => new
                {
                    d.Id,
                    d.TongTien,
                    d.TrangThai,
                    d.NgayTao,
                    SoSanPham = d.ChiTietDonHangs.Count,
                    SanPhamDauTien = d.ChiTietDonHangs
                        .Select(c => c.SanPham.TieuDe)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }

        // GET /api/don-hang/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var userId = GetCurrentUserId();
            var vaiTro = User.FindFirstValue(ClaimTypes.Role);

            var donHang = await _db.DonHangs
                .Include(d => d.NguoiMua)
                .Include(d => d.DiaChi)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                        .ThenInclude(s => s.AnhSanPhams)
                .Include(d => d.ThanhToan)
                .Include(d => d.TheoDoidonHangs)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            if (donHang.NguoiMuaId != userId && vaiTro != "admin") return Forbid();

            return Ok(new
            {
                donHang.Id,
                donHang.TongTien,
                donHang.TrangThai,
                donHang.GhiChu,
                donHang.NgayTao,
                NguoiMua = new { donHang.NguoiMua.HoTen, donHang.NguoiMua.SoDienThoai },
                DiaChi = new
                {
                    donHang.DiaChi.HoTen,
                    donHang.DiaChi.SoDienThoai,
                    donHang.DiaChi.DuongPho,
                    donHang.DiaChi.QuanHuyen,
                    donHang.DiaChi.TinhThanh
                },
                SanPhams = donHang.ChiTietDonHangs.Select(c => new
                {
                    c.SanPham.TieuDe,
                    c.GiaTaiThoiDiem,
                    c.SoLuong,
                    ThanhTien = c.GiaTaiThoiDiem * c.SoLuong,
                    Anh = c.SanPham.AnhSanPhams.Where(a => a.LaAnhDaiDien).Select(a => a.DuongDanAnh).FirstOrDefault()
                }),
                ThanhToan = new
                {
                    donHang.ThanhToan.PhuongThuc,
                    donHang.ThanhToan.SoTien,
                    donHang.ThanhToan.TrangThai
                },
<<<<<<< HEAD
                LichSuTrangThai = donHang.TheoDoidonHangs.OrderBy(t => t.NgayCapNhat)
            });
        }

=======
                LichSuTrangThai = donHang.TheoDoidonHangs
                    .OrderBy(t => t.NgayCapNhat)
                    .Select(t => new { t.Id, t.TrangThai, t.GhiChu, t.NgayCapNhat })
            });
        }

        // GET /api/don-hang/ban-cua-toi
        [HttpGet("ban-cua-toi")]
        public async Task<IActionResult> DonHangBanCuaToi([FromQuery] int trang = 1, [FromQuery] int soLuong = 10)
        {
            var sellerId = GetCurrentUserId();

            var query = _db.DonHangs
                .Include(d => d.NguoiMua)
                .Include(d => d.DiaChi)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                        .ThenInclude(s => s.AnhSanPhams)
                .Where(d => d.ChiTietDonHangs.Any(c => c.SanPham.NguoiBanId == sellerId));

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(d => new
                {
                    d.Id,
                    d.TrangThai,
                    d.NgayTao,
                    NguoiMua = d.NguoiMua.HoTen,
                    SoSanPhamCuaBan = d.ChiTietDonHangs.Count(c => c.SanPham.NguoiBanId == sellerId),
                    TongTienCuaBan = d.ChiTietDonHangs
                        .Where(c => c.SanPham.NguoiBanId == sellerId)
                        .Sum(c => c.GiaTaiThoiDiem * c.SoLuong),
                    SanPhamDauTien = d.ChiTietDonHangs
                        .Where(c => c.SanPham.NguoiBanId == sellerId)
                        .Select(c => c.SanPham.TieuDe)
                        .FirstOrDefault(),
                    AnhDauTien = d.ChiTietDonHangs
                        .Where(c => c.SanPham.NguoiBanId == sellerId)
                        .Select(c => c.SanPham.AnhSanPhams.Where(a => a.LaAnhDaiDien).Select(a => a.DuongDanAnh).FirstOrDefault())
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }

        // GET /api/don-hang/ban-cua-toi/{id}
        [HttpGet("ban-cua-toi/{id}")]
        public async Task<IActionResult> ChiTietDonHangBan(Guid id)
        {
            var sellerId = GetCurrentUserId();

            var donHang = await _db.DonHangs
                .Include(d => d.NguoiMua)
                .Include(d => d.DiaChi)
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                        .ThenInclude(s => s.AnhSanPhams)
                .Include(d => d.ThanhToan)
                .Include(d => d.TheoDoidonHangs)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            var coSanPhamCuaBan = donHang.ChiTietDonHangs.Any(c => c.SanPham.NguoiBanId == sellerId);
            if (!coSanPhamCuaBan) return Forbid();

            var sanPhamsCuaBan = donHang.ChiTietDonHangs
                .Where(c => c.SanPham.NguoiBanId == sellerId)
                .Select(c => new
                {
                    c.SanPhamId,
                    c.SanPham.TieuDe,
                    c.GiaTaiThoiDiem,
                    c.SoLuong,
                    ThanhTien = c.GiaTaiThoiDiem * c.SoLuong,
                    Anh = c.SanPham.AnhSanPhams.Where(a => a.LaAnhDaiDien).Select(a => a.DuongDanAnh).FirstOrDefault()
                })
                .ToList();

            var tongTienCuaBan = sanPhamsCuaBan.Sum(x => x.ThanhTien);

            return Ok(new
            {
                donHang.Id,
                donHang.TrangThai,
                donHang.NgayTao,
                TongTienCuaBan = tongTienCuaBan,
                donHang.GhiChu,
                NguoiMua = new { donHang.NguoiMua.HoTen, donHang.NguoiMua.SoDienThoai },
                DiaChi = new
                {
                    donHang.DiaChi.HoTen,
                    donHang.DiaChi.SoDienThoai,
                    donHang.DiaChi.DuongPho,
                    donHang.DiaChi.QuanHuyen,
                    donHang.DiaChi.TinhThanh
                },
                SanPhams = sanPhamsCuaBan,
                ThanhToan = new
                {
                    donHang.ThanhToan.PhuongThuc,
                    donHang.ThanhToan.SoTien,
                    donHang.ThanhToan.TrangThai
                },
                LichSuTrangThai = donHang.TheoDoidonHangs
                    .OrderBy(t => t.NgayCapNhat)
                    .Select(t => new { t.Id, t.TrangThai, t.GhiChu, t.NgayCapNhat })
            });
        }

        // PUT /api/don-hang/ban-cua-toi/{id}/chap-nhan
        [HttpPut("ban-cua-toi/{id}/chap-nhan")]
        public async Task<IActionResult> ChapNhanDonHangBan(Guid id)
        {
            var sellerId = GetCurrentUserId();

            var donHang = await _db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                    .ThenInclude(c => c.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });

            var coSanPhamCuaBan = donHang.ChiTietDonHangs.Any(c => c.SanPham.NguoiBanId == sellerId);
            if (!coSanPhamCuaBan) return Forbid();

            if (donHang.TrangThai != "cho_xac_nhan")
                return BadRequest(new { message = "Chỉ có thể chấp nhận đơn hàng đang chờ xác nhận" });

            // Tạm thời chỉ hỗ trợ đơn hàng 1 người bán
            var nhieuNguoiBan = donHang.ChiTietDonHangs
                .Select(c => c.SanPham.NguoiBanId)
                .Distinct()
                .Count() > 1;

            if (nhieuNguoiBan)
                return BadRequest(new { message = "Đơn hàng có nhiều người bán, hiện chưa hỗ trợ chấp nhận theo từng người bán" });

            donHang.TrangThai = "dang_giao";
            donHang.NgayCapNhat = DateTime.Now;

            await _db.TheoDoidonHangs.AddAsync(new TheoDoidonHang
            {
                Id = Guid.NewGuid(),
                DonHangId = id,
                TrangThai = "dang_giao",
                GhiChu = "Người bán đã chấp nhận đơn",
                NgayCapNhat = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã chấp nhận đơn hàng" });
        }

>>>>>>> origin/Tuannnk
        // PUT /api/don-hang/{id}/huy
        [HttpPut("{id}/huy")]
        public async Task<IActionResult> HuyDonHang(Guid id)
        {
            var userId = GetCurrentUserId();
            var donHang = await _db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            if (donHang.NguoiMuaId != userId) return Forbid();
            if (donHang.TrangThai != "cho_xac_nhan")
                return BadRequest(new { message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận" });

            // Hoàn lại số lượng tồn kho
            foreach (var item in donHang.ChiTietDonHangs)
            {
                var sanPham = await _db.SanPhams.FindAsync(item.SanPhamId);
                if (sanPham != null)
                {
                    sanPham.SoLuong += item.SoLuong;
                    sanPham.TrangThai = "dang_ban";
                }
            }

            donHang.TrangThai = "da_huy";
            donHang.NgayCapNhat = DateTime.Now;

            await _db.TheoDoidonHangs.AddAsync(new TheoDoidonHang
            {
                Id = Guid.NewGuid(),
                DonHangId = id,
                TrangThai = "da_huy",
                GhiChu = "Người mua đã hủy đơn hàng",
                NgayCapNhat = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = "Hủy đơn hàng thành công" });
        }

        // PUT /api/don-hang/{id}/xac-nhan (Admin)
        [HttpPut("{id}/xac-nhan")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> XacNhan(Guid id) =>
            await CapNhatTrangThai(id, "cho_xac_nhan", "dang_giao", "Đơn hàng đã được xác nhận");

        // PUT /api/don-hang/{id}/dang-giao (Admin)
        [HttpPut("{id}/dang-giao")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DangGiao(Guid id) =>
            await CapNhatTrangThai(id, "dang_giao", "da_giao", "Đơn hàng đang được giao");

        // PUT /api/don-hang/{id}/da-giao (Admin)
        [HttpPut("{id}/da-giao")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DaGiao(Guid id) =>
            await CapNhatTrangThai(id, "da_giao", "hoan_thanh", "Đơn hàng đã giao thành công");

        private async Task<IActionResult> CapNhatTrangThai(Guid id, string trangThaiHienTai, string trangThaiMoi, string ghiChu)
        {
            var donHang = await _db.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound(new { message = "Không tìm thấy đơn hàng" });
            if (donHang.TrangThai != trangThaiHienTai)
                return BadRequest(new { message = $"Đơn hàng không ở trạng thái '{trangThaiHienTai}'" });

            donHang.TrangThai = trangThaiMoi;
            donHang.NgayCapNhat = DateTime.Now;

            await _db.TheoDoidonHangs.AddAsync(new TheoDoidonHang
            {
                Id = Guid.NewGuid(),
                DonHangId = id,
                TrangThai = trangThaiMoi,
                GhiChu = ghiChu,
                NgayCapNhat = DateTime.Now
            });

            await _db.SaveChangesAsync();
            return Ok(new { message = ghiChu });
        }

        // GET /api/don-hang (Admin)
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TatCaDonHang(
            [FromQuery] string? trangThai,
            [FromQuery] int trang = 1,
            [FromQuery] int soLuong = 10)
        {
            var query = _db.DonHangs
                .Include(d => d.NguoiMua)
                .AsQueryable();

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(d => d.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(d => new
                {
                    d.Id,
                    d.TongTien,
                    d.TrangThai,
                    d.NgayTao,
                    NguoiMua = d.NguoiMua.HoTen,
                    SoSanPham = d.ChiTietDonHangs.Count
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/Tuannnk
