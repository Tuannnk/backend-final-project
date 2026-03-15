namespace bandothanhli.Models
{
    public class TheoDoidonHang
    {
        public Guid Id { get; set; }
        public Guid DonHangId { get; set; }
        public string TrangThai { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        public DonHang DonHang { get; set; }
    }
}
