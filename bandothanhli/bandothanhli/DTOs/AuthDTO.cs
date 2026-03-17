namespace bandothanhli.DTOs
{
    public class DangKyDTO
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string MatKhau { get; set; }
    }

    public class XacMinhOtpDTO
    {
        public string Email { get; set; }
        public string MaOtp { get; set; }
    }

    public class DangNhapDTO
    {
        public string Email { get; set; }
        public string MatKhau { get; set; }
    }

    public class YeuCauDoiMatKhauDTO
    {
        public string Email { get; set; }
    }

    public class DoiMatKhauDTO
    {
        public string Email { get; set; }
        public string MaOtp { get; set; }
        public string MatKhauMoi { get; set; }
    }
}