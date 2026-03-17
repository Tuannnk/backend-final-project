using bandothanhli.Data;
using bandothanhli.DTOs;
using bandothanhli.Models;
using bandothanhli.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IOtpService _otpService;

        public AuthController(AppDbContext db, IEmailService emailService, IJwtService jwtService, IOtpService otpService)
        {
            _db = db;
            _emailService = emailService;
            _jwtService = jwtService;
            _otpService = otpService;
        }

        // BƯỚC 1: Đăng ký - gửi OTP
        [HttpPost("dang-ky")]
        public async Task<IActionResult> DangKy([FromBody] DangKyDTO dto)
        {
            if (await _db.NguoiDungs.AnyAsync(n => n.Email == dto.Email))
                return BadRequest(new { message = "Email đã được sử dụng" });

            // Tạo OTP
            var maOtp = _otpService.TaoMaOtp();
            var otp = new OtpXacMinh
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                MaOtp = maOtp,
                MucDich = "dang_ky",
                NgayHetHan = DateTime.Now.AddMinutes(5),
                DaSuDung = false
            };

            // Lưu thông tin tạm vào OTP (dùng để tạo user sau khi xác minh)
            // Lưu user chưa xác thực
            var nguoiDung = new NguoiDung
            {
                Id = Guid.NewGuid(),
                HoTen = dto.HoTen,
                Email = dto.Email,
                SoDienThoai = dto.SoDienThoai,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                VaiTro = "nguoi_dung",
                DaXacThuc = false,
                NgayTao = DateTime.Now
            };

            await _db.NguoiDungs.AddAsync(nguoiDung);
            await _db.OtpXacMinhs.AddAsync(otp);
            await _db.SaveChangesAsync();

            // Gửi email
            var noiDung = _otpService.TaoNoiDungEmail(dto.HoTen, maOtp, "dang_ky");
            await _emailService.GuiEmailAsync(dto.Email, "Xác minh đăng ký tài khoản", noiDung);

            return Ok(new { message = "Mã OTP đã được gửi về email của bạn" });
        }

        // BƯỚC 2: Xác minh OTP đăng ký
        [HttpPost("xac-minh-otp")]
        public async Task<IActionResult> XacMinhOtp([FromBody] XacMinhOtpDTO dto)
        {
            var otp = await _db.OtpXacMinhs
                .Where(o => o.Email == dto.Email
                    && o.MaOtp == dto.MaOtp
                    && o.MucDich == "dang_ky"
                    && !o.DaSuDung
                    && o.NgayHetHan > DateTime.Now)
                .FirstOrDefaultAsync();

            if (otp == null)
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });

            // Kích hoạt tài khoản
            var nguoiDung = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == dto.Email);
            if (nguoiDung == null)
                return NotFound(new { message = "Không tìm thấy tài khoản" });

            nguoiDung.DaXacThuc = true;
            otp.DaSuDung = true;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Xác minh thành công! Bạn có thể đăng nhập." });
        }

        // Đăng nhập
        [HttpPost("dang-nhap")]
        public async Task<IActionResult> DangNhap([FromBody] DangNhapDTO dto)
        {
            var nguoiDung = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == dto.Email);

            if (nguoiDung == null || !BCrypt.Net.BCrypt.Verify(dto.MatKhau, nguoiDung.MatKhau))
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });

            if (!nguoiDung.DaXacThuc)
                return Unauthorized(new { message = "Tài khoản chưa được xác minh. Vui lòng kiểm tra email." });

            var token = _jwtService.TaoToken(nguoiDung);
            Response.Cookies.Append("access_token", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddDays(7),
                Path = "/"
            });

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token,
                nguoiDung = new
                {
                    nguoiDung.Id,
                    nguoiDung.HoTen,
                    nguoiDung.Email,
                    nguoiDung.VaiTro
                }
            });
        }

        // Đăng xuất (xóa cookie token)
        [HttpPost("dang-xuat")]
        public IActionResult DangXuat()
        {
            Response.Cookies.Delete("access_token", new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });
            return Ok(new { message = "Đăng xuất thành công" });
        }

        // Đồng bộ token từ Authorization header vào cookie để dùng cho các trang MVC (vd: /admin/*)
        [HttpPost("refresh-cookie")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult RefreshCookie()
        {
            var auth = Request.Headers.Authorization.ToString();
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Thiếu token" });

            var token = auth.Substring("Bearer ".Length).Trim();
            Response.Cookies.Append("access_token", token, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddDays(7),
                Path = "/"
            });

            return Ok(new { message = "OK" });
        }

        // BƯỚC 1: Yêu cầu đổi mật khẩu - gửi OTP
        [HttpPost("yeu-cau-doi-mat-khau")]
        public async Task<IActionResult> YeuCauDoiMatKhau([FromBody] YeuCauDoiMatKhauDTO dto)
        {
            var nguoiDung = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == dto.Email);
            if (nguoiDung == null)
                return NotFound(new { message = "Không tìm thấy tài khoản với email này" });

            var maOtp = _otpService.TaoMaOtp();
            var otp = new OtpXacMinh
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                MaOtp = maOtp,
                MucDich = "doi_mat_khau",
                NgayHetHan = DateTime.Now.AddMinutes(5),
                DaSuDung = false
            };

            await _db.OtpXacMinhs.AddAsync(otp);
            await _db.SaveChangesAsync();

            var noiDung = _otpService.TaoNoiDungEmail(nguoiDung.HoTen, maOtp, "doi_mat_khau");
            await _emailService.GuiEmailAsync(dto.Email, "Xác minh đổi mật khẩu", noiDung);

            return Ok(new { message = "Mã OTP đã được gửi về email của bạn" });
        }

        // BƯỚC 2: Xác minh OTP và đổi mật khẩu
        [HttpPost("doi-mat-khau")]
        public async Task<IActionResult> DoiMatKhau([FromBody] DoiMatKhauDTO dto)
        {
            var otp = await _db.OtpXacMinhs
                .Where(o => o.Email == dto.Email
                    && o.MaOtp == dto.MaOtp
                    && o.MucDich == "doi_mat_khau"
                    && !o.DaSuDung
                    && o.NgayHetHan > DateTime.Now)
                .FirstOrDefaultAsync();

            if (otp == null)
                return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn" });

            var nguoiDung = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == dto.Email);
            if (nguoiDung == null)
                return NotFound(new { message = "Không tìm thấy tài khoản" });

            nguoiDung.MatKhau = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);
            otp.DaSuDung = true;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }
    }
}
