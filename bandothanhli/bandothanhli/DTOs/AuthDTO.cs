using System.ComponentModel.DataAnnotations;

namespace bandothanhli.DTOs
{
    public class DangKyDTO
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 60 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ (vd: 0901234567)")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ và 1 số")]
        public string MatKhau { get; set; } = string.Empty;
    }

    public class XacMinhOtpDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải đủ 6 số")]
        public string MaOtp { get; set; } = string.Empty;
    }

    public class DangNhapDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string MatKhau { get; set; } = string.Empty;
    }

    public class YeuCauDoiMatKhauDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }

    public class DoiMatKhauDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã OTP là bắt buộc")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã OTP phải đủ 6 số")]
        public string MaOtp { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{6,}$", ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ và 1 số")]
        public string MatKhauMoi { get; set; } = string.Empty;
    }
}

