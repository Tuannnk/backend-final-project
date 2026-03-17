using bandothanhli.Data;
using bandothanhli.Models;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Services
{
    public interface IDataSeederService
    {
        Task SeedDataAsync();
    }

    public class DataSeederService : IDataSeederService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DataSeederService> _logger;

        public DataSeederService(AppDbContext db, IWebHostEnvironment env, ILogger<DataSeederService> logger)
        {
            _db = db;
            _env = env;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                // Ki?m tra xem ?ã có d? li?u không
                if (await _db.SanPhams.AnyAsync())
                {
                    _logger.LogInformation("Database already has data. Skipping seed.");
                    return;
                }

                _logger.LogInformation("Starting data seed...");

                // T?o ng??i dùng
                var user = new NguoiDung
                {
                    Id = Guid.NewGuid(),
                    HoTen = "Nguy?n V?n A",
                    Email = "user@example.com",
                    SoDienThoai = "0912345678",
                    MatKhau = "hashed_password",
                    VaiTro = "seller",
                    DaXacThuc = true,
                    NgayTao = DateTime.Now
                };

                await _db.NguoiDungs.AddAsync(user);

                // T?o danh m?c
                var category = new DanhMuc
                {
                    Id = Guid.NewGuid(),
                    TenDanhMuc = "?i?n tho?i",
                    Slug = "dien-thoai",
                    IconUrl = "/images/phone.png",
                    ThuTu = 1
                };

                await _db.DanhMucs.AddAsync(category);

                // T?o tình tr?ng
                var condition = new TinhTrangSanPham
                {
                    Id = Guid.NewGuid(),
                    TenTinhTrang = "Nh? m?i",
                    DiemTinhTrang = 100,
                    MoTa = "S?n ph?m nh? m?i"
                };

                await _db.TinhTrangSanPhams.AddAsync(condition);

                await _db.SaveChangesAsync();

                // T?o s?n ph?m
                for (int i = 1; i <= 3; i++)
                {
                    var product = new SanPham
                    {
                        Id = Guid.NewGuid(),
                        NguoiBanId = user.Id,
                        DanhMucId = category.Id,
                        TinhTrangId = condition.Id,
                        TieuDe = $"S?n ph?m Test {i}",
                        MoTa = $"?ây là mô t? s?n ph?m test s? {i}",
                        Gia = 1000000 + (i * 100000),
                        GiaGoc = 1200000 + (i * 100000),
                        SoLuong = 5,
                        TrangThai = "dang_ban",
                        DiaDiem = "Hà N?i",
                        NgayTao = DateTime.Now.AddDays(-i)
                    };

                    await _db.SanPhams.AddAsync(product);
                    await _db.SaveChangesAsync();

                    // Thêm ?nh cho s?n ph?m
                    var image = new AnhSanPham
                    {
                        Id = Guid.NewGuid(),
                        SanPhamId = product.Id,
                        DuongDanAnh = i % 2 == 1 ? "/images/picture1.png" : "/images/picture2.png",
                        LaAnhDaiDien = true,
                        ThuTu = 1
                    };

                    await _db.AnhSanPhams.AddAsync(image);
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation("Data seed completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during data seed: {ex.Message}");
                throw;
            }
        }
    }
}
