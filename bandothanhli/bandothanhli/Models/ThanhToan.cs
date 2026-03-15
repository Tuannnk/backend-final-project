namespace bandothanhli.Models
{
    public class ThanhToan
    {
        public Guid Id { get; set; }
        public Guid DonHangId { get; set; }
        public string PhuongThuc { get; set; } // "tien_mat", "chuyen_khoan", "vi_dien_tu"
        public decimal SoTien { get; set; }
        public string TrangThai { get; set; } // "cho_thanh_toan", "da_thanh_toan", "that_bai"
        public string MaGiaoDich { get; set; }
        public DateTime? NgayThanhToan { get; set; }

        public DonHang DonHang { get; set; }
    }
}
