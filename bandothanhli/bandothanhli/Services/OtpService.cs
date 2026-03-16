namespace bandothanhli.Services
{
    public interface IOtpService
    {
        string TaoMaOtp();
        string TaoNoiDungEmail(string hoTen, string maOtp, string mucDich);
    }

    public class OtpService : IOtpService
    {
        public string TaoMaOtp()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public string TaoNoiDungEmail(string hoTen, string maOtp, string mucDich)
        {
            var tieuDe = mucDich == "dang_ky" ? "Xác minh đăng ký tài khoản" : "Xác minh đổi mật khẩu";
            return $@"
                <h2>{tieuDe}</h2>
                <p>Xin chào <b>{hoTen}</b>,</p>
                <p>Mã OTP của bạn là:</p>
                <h1 style='color: #e74c3c; letter-spacing: 8px;'>{maOtp}</h1>
                <p>Mã có hiệu lực trong <b>5 phút</b>.</p>
                <p>Nếu bạn không thực hiện yêu cầu này, hãy bỏ qua email này.</p>
            ";
        }
    }
}