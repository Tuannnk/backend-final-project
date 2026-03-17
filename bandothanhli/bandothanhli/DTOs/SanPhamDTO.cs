namespace bandothanhli.DTOs
{
    public class TaoSanPhamDTO
    {
        public Guid DanhMucId { get; set; }
        public Guid TinhTrangId { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public decimal Gia { get; set; }
        public decimal GiaGoc { get; set; }
        public int SoLuong { get; set; }
        public string DiaDiem { get; set; }
    }
}