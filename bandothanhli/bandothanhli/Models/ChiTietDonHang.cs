namespace bandothanhli.Models
{
    public class ChiTietDonHang
    {
        public Guid Id { get; set; }
        public Guid DonHangId { get; set; }
        public Guid SanPhamId { get; set; }
        public decimal GiaTaiThoiDiem { get; set; }
        public int SoLuong { get; set; }

        public DonHang DonHang { get; set; }
        public SanPham SanPham { get; set; }
    }
}
