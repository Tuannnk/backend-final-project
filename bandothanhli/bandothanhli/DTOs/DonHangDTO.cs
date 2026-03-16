namespace bandothanhli.DTOs
{
    public class TaoDonHangDTO
    {
        public Guid DiaChiId { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string? GhiChu { get; set; }
        public List<SanPhamDonHangDTO> SanPhams { get; set; }
    }

    public class SanPhamDonHangDTO
    {
        public Guid SanPhamId { get; set; }
        public int SoLuong { get; set; }
    }
}