using bandothanhli.Data;
using bandothanhli.DTOs;
using bandothanhli.Models;
<<<<<<< HEAD
=======
using bandothanhli.Services;
>>>>>>> origin/Tuannnk
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/san-pham")]
    public class SanPhamController : ControllerBase
    {
        private readonly AppDbContext _db;
<<<<<<< HEAD
        private readonly IWebHostEnvironment _env;

        public SanPhamController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
=======
        private readonly ICloudinaryService _cloudinary;

        public SanPhamController(AppDbContext db, ICloudinaryService cloudinary)
        {
            _db = db;
            _cloudinary = cloudinary;
>>>>>>> origin/Tuannnk
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

<<<<<<< HEAD
        // POST /api/san-pham/upload-anh
        [HttpPost("upload-anh")]
        [Authorize]
        public async Task<IActionResult> UploadAnh([FromForm] UploadAnhDTO dto)
        {
            try
            {
                if (dto.AnhFile == null || dto.AnhFile.Length == 0)
                    return BadRequest(new { message = "Vui lòng chọn hình ảnh" });

                // Kiểm tra loại file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(dto.AnhFile.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (.jpg, .png, .gif, .webp)" });

                // Kiểm tra kích thước (max 5MB)
                if (dto.AnhFile.Length > 5 * 1024 * 1024)
                    return BadRequest(new { message = "Kích thước ảnh không được vượt quá 5MB" });

                var userId = GetCurrentUserId();
                var sanPham = await _db.SanPhams.FindAsync(dto.SanPhamId);
                
                if (sanPham == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm" });

                if (sanPham.NguoiBanId != userId)
                    return Forbid();

                // Tạo thư mục nếu chưa tồn tại
                var uploadsPath = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);

                // Tạo tên file duy nhất
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.AnhFile.CopyToAsync(stream);
                }

                // Nếu là ảnh đại diện, bỏ chọn ảnh đại diện cũ
                if (dto.LaAnhDaiDien)
                {
                    var anhCuDaiDien = await _db.AnhSanPhams
                        .Where(a => a.SanPhamId == dto.SanPhamId && a.LaAnhDaiDien)
                        .FirstOrDefaultAsync();
                    
                    if (anhCuDaiDien != null)
                        anhCuDaiDien.LaAnhDaiDien = false;
                }

                // Lưu thông tin ảnh vào database
                var anh = new AnhSanPham
                {
                    Id = Guid.NewGuid(),
                    SanPhamId = dto.SanPhamId,
                    DuongDanAnh = $"/images/{fileName}",
                    LaAnhDaiDien = dto.LaAnhDaiDien,
                    ThuTu = await _db.AnhSanPhams.Where(a => a.SanPhamId == dto.SanPhamId).CountAsync()
                };

                await _db.AnhSanPhams.AddAsync(anh);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Upload ảnh thành công", id = anh.Id, duongDanAnh = anh.DuongDanAnh });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi upload ảnh: {ex.Message}" });
            }
        }

        // DELETE /api/san-pham/xoa-anh/{id}
        [HttpDelete("xoa-anh/{id}")]
        [Authorize]
        public async Task<IActionResult> XoaAnh(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var anh = await _db.AnhSanPhams
                    .Include(a => a.SanPham)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (anh == null)
                    return NotFound(new { message = "Không tìm thấy ảnh" });

                if (anh.SanPham.NguoiBanId != userId)
                    return Forbid();

                // Xóa file khỏi disk
                var filePath = Path.Combine(_env.WebRootPath, anh.DuongDanAnh.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                _db.AnhSanPhams.Remove(anh);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Xóa ảnh thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi xóa ảnh: {ex.Message}" });
            }
        }

=======
>>>>>>> origin/Tuannnk
        // GET /api/san-pham
        [HttpGet]
        public async Task<IActionResult> DanhSach(
            [FromQuery] Guid? danhMucId,
            [FromQuery] Guid? tinhTrangId,
            [FromQuery] string? tuKhoa,
            [FromQuery] decimal? giaMin,
            [FromQuery] decimal? giaMax,
            [FromQuery] int trang = 1,
            [FromQuery] int soLuong = 12)
        {
            var query = _db.SanPhams
                .Where(s => s.TrangThai == "dang_ban")
                .Include(s => s.NguoiBan)
                .Include(s => s.DanhMuc)
                .Include(s => s.TinhTrang)
                .Include(s => s.AnhSanPhams)
<<<<<<< HEAD
=======
                .Include(s => s.DanhGias)
>>>>>>> origin/Tuannnk
                .AsQueryable();

            if (danhMucId.HasValue)
                query = query.Where(s => s.DanhMucId == danhMucId);

            if (tinhTrangId.HasValue)
                query = query.Where(s => s.TinhTrangId == tinhTrangId);

            if (!string.IsNullOrEmpty(tuKhoa))
                query = query.Where(s => s.TieuDe.Contains(tuKhoa) || s.MoTa.Contains(tuKhoa));

            if (giaMin.HasValue)
                query = query.Where(s => s.Gia >= giaMin);

            if (giaMax.HasValue)
                query = query.Where(s => s.Gia <= giaMax);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(s => s.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(s => new
                {
                    s.Id,
                    s.TieuDe,
                    s.Gia,
                    s.GiaGoc,
                    s.DiaDiem,
                    s.NgayTao,
                    TinhTrang = s.TinhTrang.TenTinhTrang,
                    DanhMuc = s.DanhMuc.TenDanhMuc,
                    NguoiBan = s.NguoiBan.HoTen,
<<<<<<< HEAD
=======
                    DiemDanhGia = s.DanhGias.Any() ? s.DanhGias.Average(d => (double)d.DiemDanhGia) : 0,
                    SoDanhGia = s.DanhGias.Count,
>>>>>>> origin/Tuannnk
                    AnhDaiDien = s.AnhSanPhams
                        .Where(a => a.LaAnhDaiDien)
                        .Select(a => a.DuongDanAnh)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }

        // GET /api/san-pham/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> ChiTiet(Guid id)
        {
            var sanPham = await _db.SanPhams
                .Include(s => s.NguoiBan)
                .Include(s => s.DanhMuc)
                .Include(s => s.TinhTrang)
                .Include(s => s.AnhSanPhams)
                .Include(s => s.DanhGias)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

            return Ok(new
            {
                sanPham.Id,
<<<<<<< HEAD
=======
                sanPham.DanhMucId,
                sanPham.TinhTrangId,
>>>>>>> origin/Tuannnk
                sanPham.TieuDe,
                sanPham.MoTa,
                sanPham.Gia,
                sanPham.GiaGoc,
                sanPham.SoLuong,
                sanPham.TrangThai,
                sanPham.DiaDiem,
                sanPham.NgayTao,
                TinhTrang = sanPham.TinhTrang.TenTinhTrang,
                DanhMuc = sanPham.DanhMuc.TenDanhMuc,
<<<<<<< HEAD
                NguoiBan = new { sanPham.NguoiBan.Id, sanPham.NguoiBan.HoTen, sanPham.NguoiBan.SoDienThoai },
=======
                NguoiBan = new { sanPham.NguoiBan.Id, sanPham.NguoiBan.HoTen, sanPham.NguoiBan.Email, sanPham.NguoiBan.SoDienThoai },
>>>>>>> origin/Tuannnk
                Anh = sanPham.AnhSanPhams.OrderBy(a => a.ThuTu).Select(a => new { a.Id, a.DuongDanAnh, a.LaAnhDaiDien }),
                DiemDanhGia = sanPham.DanhGias.Any() ? sanPham.DanhGias.Average(d => d.DiemDanhGia) : 0,
                SoDanhGia = sanPham.DanhGias.Count
            });
        }

<<<<<<< HEAD
=======
        // POST /api/san-pham/{id}/anh
        [HttpPost("{id}/anh")]
        [Authorize]
        [RequestSizeLimit(30_000_000)]
        public async Task<IActionResult> TaiAnh(Guid id, [FromForm] List<IFormFile> files, [FromForm] int? anhDaiDienIndex)
        {
            if (files == null || files.Count == 0) return BadRequest(new { message = "Vui lòng chọn ít nhất 1 ảnh" });
            if (files.Count > 10) return BadRequest(new { message = "Tối đa 10 ảnh mỗi lần" });

            var userId = GetCurrentUserId();
            var sanPham = await _db.SanPhams
                .Include(s => s.AnhSanPhams)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            if (sanPham.NguoiBanId != userId) return Forbid();

            var startOrder = sanPham.AnhSanPhams.Any() ? sanPham.AnhSanPhams.Max(a => a.ThuTu) + 1 : 1;
            var uploaded = new List<AnhSanPham>();

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];
                if (file.Length <= 0) return BadRequest(new { message = "Có file rỗng" });
                if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { message = "Chỉ hỗ trợ file ảnh" });

                string url;
                try
                {
                    url = await _cloudinary.UploadImageAsync(file, HttpContext.RequestAborted);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = ex.Message });
                }
                var anh = new AnhSanPham
                {
                    Id = Guid.NewGuid(),
                    SanPhamId = sanPham.Id,
                    DuongDanAnh = url,
                    LaAnhDaiDien = false,
                    ThuTu = startOrder + i
                };
                uploaded.Add(anh);
            }

            var idx = anhDaiDienIndex ?? 0;
            if (idx < 0 || idx >= uploaded.Count) idx = 0;
            foreach (var a in sanPham.AnhSanPhams) a.LaAnhDaiDien = false;
            uploaded[idx].LaAnhDaiDien = true;

            await _db.AnhSanPhams.AddRangeAsync(uploaded);
            await _db.SaveChangesAsync();

            var result = await _db.AnhSanPhams
                .Where(a => a.SanPhamId == sanPham.Id)
                .OrderBy(a => a.ThuTu)
                .Select(a => new { a.Id, a.DuongDanAnh, a.LaAnhDaiDien, a.ThuTu })
                .ToListAsync();

            return Ok(new { message = "Tải ảnh thành công", data = result });
        }

>>>>>>> origin/Tuannnk
        // GET /api/san-pham/cua-toi
        [HttpGet("cua-toi")]
        [Authorize]
        public async Task<IActionResult> SanPhamCuaToi([FromQuery] int trang = 1, [FromQuery] int soLuong = 12)
        {
            var id = GetCurrentUserId();
            var query = _db.SanPhams
                .Where(s => s.NguoiBanId == id)
                .Include(s => s.AnhSanPhams)
                .Include(s => s.TinhTrang);

            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(s => s.NgayTao)
                .Skip((trang - 1) * soLuong)
                .Take(soLuong)
                .Select(s => new
                {
                    s.Id,
                    s.TieuDe,
                    s.Gia,
                    s.SoLuong,
                    s.TrangThai,
                    s.NgayTao,
                    TinhTrang = s.TinhTrang.TenTinhTrang,
                    AnhDaiDien = s.AnhSanPhams
                        .Where(a => a.LaAnhDaiDien)
                        .Select(a => a.DuongDanAnh)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, trang, soLuong, data });
        }

        // POST /api/san-pham
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Tao([FromBody] TaoSanPhamDTO dto)
        {
            var id = GetCurrentUserId();

            var sanPham = new SanPham
            {
                Id = Guid.NewGuid(),
                NguoiBanId = id,
                DanhMucId = dto.DanhMucId,
                TinhTrangId = dto.TinhTrangId,
                TieuDe = dto.TieuDe,
                MoTa = dto.MoTa,
                Gia = dto.Gia,
                GiaGoc = dto.GiaGoc,
                SoLuong = dto.SoLuong,
                DiaDiem = dto.DiaDiem,
                TrangThai = "dang_ban",
                NgayTao = DateTime.Now
            };

            await _db.SanPhams.AddAsync(sanPham);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đăng sản phẩm thành công", id = sanPham.Id });
        }

        // PUT /api/san-pham/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> CapNhat(Guid id, [FromBody] TaoSanPhamDTO dto)
        {
            var userId = GetCurrentUserId();
            var sanPham = await _db.SanPhams.FindAsync(id);

            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            if (sanPham.NguoiBanId != userId) return Forbid();

            sanPham.TieuDe = dto.TieuDe ?? sanPham.TieuDe;
            sanPham.MoTa = dto.MoTa ?? sanPham.MoTa;
            sanPham.Gia = dto.Gia;
            sanPham.GiaGoc = dto.GiaGoc;
            sanPham.SoLuong = dto.SoLuong;
            sanPham.DiaDiem = dto.DiaDiem ?? sanPham.DiaDiem;
            sanPham.DanhMucId = dto.DanhMucId;
            sanPham.TinhTrangId = dto.TinhTrangId;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật sản phẩm thành công" });
        }

        // PUT /api/san-pham/{id}/an
        [HttpPut("{id}/an")]
        [Authorize]
        public async Task<IActionResult> AnHienSanPham(Guid id)
        {
            var userId = GetCurrentUserId();
            var sanPham = await _db.SanPhams.FindAsync(id);

            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            if (sanPham.NguoiBanId != userId) return Forbid();

            sanPham.TrangThai = sanPham.TrangThai == "dang_ban" ? "an" : "dang_ban";
            await _db.SaveChangesAsync();

            var trangThai = sanPham.TrangThai == "dang_ban" ? "hiện" : "ẩn";
            return Ok(new { message = $"Đã {trangThai} sản phẩm" });
        }

        // DELETE /api/san-pham/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var userId = GetCurrentUserId();
            var vaiTro = User.FindFirstValue(ClaimTypes.Role);
            var sanPham = await _db.SanPhams.FindAsync(id);

            if (sanPham == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
            if (sanPham.NguoiBanId != userId && vaiTro != "admin") return Forbid();

            _db.SanPhams.Remove(sanPham);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Xóa sản phẩm thành công" });
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> origin/Tuannnk
