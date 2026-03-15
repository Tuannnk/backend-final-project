namespace bandothanhli.Models
{
    public class DiaChi
    {
        public Guid Id { get; set; }
        public Guid NguoiDungId { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string DuongPho { get; set; }
        public string QuanHuyen { get; set; }
        public string TinhThanh { get; set; }
        public bool LaMacDinh { get; set; }

        public NguoiDung NguoiDung { get; set; }
        public ICollection<DonHang> DonHangs { get; set; }
    }
}
