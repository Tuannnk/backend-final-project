namespace bandothanhli.Models
{
    public class TinNhan
    {
        public Guid Id { get; set; }
        public Guid NguoiGuiId { get; set; }
        public Guid NguoiNhanId { get; set; }
        public Guid SanPhamId { get; set; }
        public string NoiDung { get; set; }
        public bool DaDoc { get; set; }
        public DateTime NgayGui { get; set; } = DateTime.Now;

        public NguoiDung NguoiGui { get; set; }
        public NguoiDung NguoiNhan { get; set; }
        public SanPham SanPham { get; set; }
    }
}
