namespace bandothanhli.Models
{
    public class DanhMuc
    {
        public Guid Id { get; set; }
        public Guid? DanhMucChaId { get; set; }
        public string TenDanhMuc { get; set; }
        public string Slug { get; set; }
        public string IconUrl { get; set; }
        public int ThuTu { get; set; }

        public DanhMuc DanhMucCha { get; set; }
        public ICollection<DanhMuc> DanhMucCons { get; set; }
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
