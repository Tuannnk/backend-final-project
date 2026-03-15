namespace bandothanhli.Models
{
    public class SanPham
    {
        public Guid Id { get; set; }
        public Guid NguoiBanId { get; set; }
        public Guid DanhMucId { get; set; }
        public Guid TinhTrangId { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public decimal Gia { get; set; }
        public decimal GiaGoc { get; set; }
        public int SoLuong { get; set; }
        public string TrangThai { get; set; } // "dang_ban", "da_ban", "an"
        public string DiaDiem { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public NguoiDung NguoiBan { get; set; }
        public DanhMuc DanhMuc { get; set; }
        public TinhTrangSanPham TinhTrang { get; set; }
        public ICollection<AnhSanPham> AnhSanPhams { get; set; }
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public ICollection<DanhGia> DanhGias { get; set; }
    }
}
