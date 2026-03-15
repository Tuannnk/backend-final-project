namespace bandothanhli.Models
{
    public class TinhTrangSanPham
    {
        public Guid Id { get; set; }
        public string TenTinhTrang { get; set; } // "Nhu moi", "90%", "70%"
        public int DiemTinhTrang { get; set; }
        public string MoTa { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }
    }
}
