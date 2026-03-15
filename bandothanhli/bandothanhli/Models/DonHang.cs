namespace bandothanhli.Models
{
    public class DonHang
    {
        public Guid Id { get; set; }
        public Guid NguoiMuaId { get; set; }
        public Guid DiaChiId { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } // "cho_xac_nhan", "dang_giao", "da_giao", "da_huy"
        public string GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; }

        public NguoiDung NguoiMua { get; set; }
        public DiaChi DiaChi { get; set; }
        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public ThanhToan ThanhToan { get; set; }
        public ICollection<TheoDoidonHang> TheoDoidonHangs { get; set; }
    }
}
