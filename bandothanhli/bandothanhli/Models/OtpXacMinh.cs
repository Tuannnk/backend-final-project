namespace bandothanhli.Models
{
    public class OtpXacMinh
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string MaOtp { get; set; }
        public string MucDich { get; set; } // "dang_ky", "doi_mat_khau"
        public DateTime NgayHetHan { get; set; }
        public bool DaSuDung { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}