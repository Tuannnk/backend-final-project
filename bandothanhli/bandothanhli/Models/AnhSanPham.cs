namespace bandothanhli.Models
{
    public class AnhSanPham
    {
        public Guid Id { get; set; }
        public Guid SanPhamId { get; set; }
        public string DuongDanAnh { get; set; }
        public bool LaAnhDaiDien { get; set; }
        public int ThuTu { get; set; }

        public SanPham SanPham { get; set; }
    }
}
