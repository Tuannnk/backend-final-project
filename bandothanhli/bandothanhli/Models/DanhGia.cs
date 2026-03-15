namespace bandothanhli.Models
{
    public class DanhGia
    {
        public Guid Id { get; set; }
        public Guid SanPhamId { get; set; }
        public Guid NguoiDanhGiaId { get; set; }
        public int DiemDanhGia { get; set; } // 1-5
        public string BinhLuan { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public SanPham SanPham { get; set; }
        public NguoiDung NguoiDanhGia { get; set; }
    }
}
