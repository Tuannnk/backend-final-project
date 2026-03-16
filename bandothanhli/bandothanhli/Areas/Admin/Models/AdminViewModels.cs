using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace bandothanhli.Areas.Admin.Models
{
    // ══════════════════ AUTH ══════════════════
    public class DangNhapVM
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = "";
    }

    public class DoiMatKhauVM
    {
        [Required(ErrorMessage = "Nhập mật khẩu cũ")]
        [DataType(DataType.Password)]
        public string MatKhauCu { get; set; } = "";

        [Required(ErrorMessage = "Nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Tối thiểu 6 ký tự")]
        [DataType(DataType.Password)]
        public string MatKhauMoi { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhan { get; set; } = "";
    }

    // ══════════════════ NGƯỜI DÙNG ══════════════════
    public class NguoiDungVM
    {
        public Guid Id { get; set; }
        public string HoTen { get; set; } = "";
        public string Email { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string VaiTro { get; set; } = "";
        public bool DaXacThuc { get; set; }
        public DateTime NgayTao { get; set; }
        public int TongSanPham { get; set; }
        public int TongDonHang { get; set; }
    }

    // ══════════════════ DANH MỤC ══════════════════
    public class DanhMucVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [Display(Name = "Tên danh mục")]
        public string TenDanhMuc { get; set; } = "";

        [Display(Name = "Slug")]
        public string Slug { get; set; } = "";

        [Display(Name = "Icon URL")]
        public string IconUrl { get; set; } = "";

        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; } = 0;

        [Display(Name = "Danh mục cha")]
        public Guid? DanhMucChaId { get; set; }
        public string? TenDanhMucCha { get; set; }
        public int SoSanPham { get; set; }
    }

    // ══════════════════ TÌNH TRẠNG ══════════════════
    public class TinhTrangVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tình trạng")]
        [Display(Name = "Tên tình trạng")]
        public string TenTinhTrang { get; set; } = "";

        [Range(0, 100)]
        [Display(Name = "Điểm tình trạng")]
        public int DiemTinhTrang { get; set; } = 0;

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; } = "";

        public int SoSanPham { get; set; }
    }

    // ══════════════════ SẢN PHẨM ══════════════════
    public class SanPhamVM
    {
        public Guid Id { get; set; }
        public string TieuDe { get; set; } = "";
        public decimal Gia { get; set; }
        public decimal GiaGoc { get; set; }
        public string TrangThai { get; set; } = "";
        public string DiaDiem { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public string TenNguoiBan { get; set; } = "";
        public string EmailNguoiBan { get; set; } = "";
        public string TenDanhMuc { get; set; } = "";
        public string TenTinhTrang { get; set; } = "";
        public string? AnhDaiDien { get; set; }
        public int SoDanhGia { get; set; }
        public double DiemTrungBinh { get; set; }
    }

    // ══════════════════ ĐƠN HÀNG ══════════════════
    public class DonHangVM
    {
        public Guid Id { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public string TenNguoiMua { get; set; } = "";
        public string EmailNguoiMua { get; set; } = "";
        public string TrangThaiThanhToan { get; set; } = "";
        public int SoSanPham { get; set; }
    }

    public class DonHangChiTietVM
    {
        public Guid Id { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public Guid NguoiMuaId { get; set; }
        public string TenNguoiMua { get; set; } = "";
        public string EmailNguoiMua { get; set; } = "";
        public string SdtNguoiMua { get; set; } = "";
        public string DiaChiGiao { get; set; } = "";
        public List<ChiTietDonHangVM> ChiTiet { get; set; } = new();
        public ThanhToanVM? ThanhToan { get; set; }
        public List<TheDoiVM> LichSuTheoDoi { get; set; } = new();
    }

    public class ChiTietDonHangVM
    {
        public Guid SanPhamId { get; set; }
        public string TieuDe { get; set; } = "";
        public string? AnhDaiDien { get; set; }
        public decimal GiaTaiThoiDiem { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => GiaTaiThoiDiem * SoLuong;
    }

    // ══════════════════ THANH TOÁN ══════════════════
    public class ThanhToanVM
    {
        public Guid Id { get; set; }
        public string PhuongThuc { get; set; } = "";
        public decimal SoTien { get; set; }
        public string TrangThai { get; set; } = "";
        public string MaGiaoDich { get; set; } = "";
        public DateTime? NgayThanhToan { get; set; }
    }

    public class XacNhanThanhToanVM
    {
        public Guid DonHangId { get; set; }
        public string MaGiaoDich { get; set; } = "";
    }

    // ══════════════════ THEO DÕI ══════════════════
    public class TheDoiVM
    {
        public Guid Id { get; set; }
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
        public DateTime NgayCapNhat { get; set; }
    }

    public class ThemTheDoiVM
    {
        public Guid DonHangId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
    }

    // ══════════════════ DASHBOARD + THỐNG KÊ ══════════════════
    public class DashboardVM
    {
        // Thẻ thống kê nhanh
        public int TongNguoiDung { get; set; }
        public int TongSanPham { get; set; }
        public int TongDonHang { get; set; }
        public decimal DoanhThuHomNay { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public decimal DoanhThuThangTruoc { get; set; }
        public int DonChoPhanHoi { get; set; }
        public int TinNhanChuaDoc { get; set; }

        // Đơn hàng theo trạng thái
        public int DonDaXacNhan { get; set; }
        public int DonDangGiao { get; set; }
        public int DonDaGiao { get; set; }
        public int DonHuyDon { get; set; }

        // Sản phẩm
        public int SanPhamDangBan { get; set; }
        public int SanPhamDaBan { get; set; }
        public int SanPhamAnHang { get; set; }

        // Người dùng
        public int NguoiDungMoiThangNay { get; set; }
        public int NguoiDungChuaXacThuc { get; set; }

        // Danh sách gần đây
        public List<DonHangVM> DonHangGanDay { get; set; } = new();
        public List<SanPhamVM> SanPhamGanDay { get; set; } = new();

        // Biểu đồ
        public List<DoanhThuTheoNgayVM> BieuDoDoanhThu { get; set; } = new();
        public List<SanPhamTheoLoaiVM> SanPhamTheoLoai { get; set; } = new();
    }

    public class DoanhThuTheoNgayVM
    {
        public string Ngay { get; set; } = "";
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
    }

    public class SanPhamTheoLoaiVM
    {
        public string TenDanhMuc { get; set; } = "";
        public int SoLuong { get; set; }
    }
}