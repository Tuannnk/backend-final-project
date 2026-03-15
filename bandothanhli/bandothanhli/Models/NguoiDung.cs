namespace bandothanhli.Models
{
    public class NguoiDung
    {
        public Guid Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string MatKhau { get; set; }
        public string VaiTro { get; set; }
        public bool DaXacThuc { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public ICollection<SanPham> SanPhams { get; set; }
        public ICollection<DonHang> DonHangs { get; set; }
        public ICollection<DanhGia> DanhGias { get; set; }
        public ICollection<DiaChi> DiaChis { get; set; }
        public ICollection<TinNhan> TinNhans { get; set; }
    }
}
