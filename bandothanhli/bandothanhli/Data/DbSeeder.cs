using bandothanhli.Models;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await SeedDanhMucAsync(db);
            await SeedTinhTrangAsync(db);
        }

        private static async Task SeedDanhMucAsync(AppDbContext db)
        {
            if (await db.DanhMucs.AnyAsync()) return;

            var thucPhamId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var thuCungId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var doDungId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var thoiTrangId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var sachId = Guid.Parse("55555555-5555-5555-5555-555555555555");
            var meBeId = Guid.Parse("66666666-6666-6666-6666-666666666666");
            var doDienTuId = Guid.Parse("77777777-7777-7777-7777-777777777777");
            var khacId = Guid.Parse("88888888-8888-8888-8888-888888888888");

            var data = new List<DanhMuc>
            {
                new() { Id = thucPhamId, TenDanhMuc = "Thực phẩm", Slug = "thuc-pham", IconUrl = "", ThuTu = 1, DanhMucChaId = null },
                new() { Id = thuCungId, TenDanhMuc = "Thú cưng", Slug = "thu-cung", IconUrl = "", ThuTu = 2, DanhMucChaId = null },
                new() { Id = doDungId, TenDanhMuc = "Đồ dùng", Slug = "do-dung", IconUrl = "", ThuTu = 3, DanhMucChaId = null },
                new() { Id = thoiTrangId, TenDanhMuc = "Thời trang", Slug = "thoi-trang", IconUrl = "", ThuTu = 4, DanhMucChaId = null },
                new() { Id = sachId, TenDanhMuc = "Sách & học tập", Slug = "sach-hoc-tap", IconUrl = "", ThuTu = 5, DanhMucChaId = null },
                new() { Id = meBeId, TenDanhMuc = "Mẹ & bé", Slug = "me-be", IconUrl = "", ThuTu = 6, DanhMucChaId = null },
                new() { Id = doDienTuId, TenDanhMuc = "Đồ điện tử", Slug = "do-dien-tu", IconUrl = "", ThuTu = 7, DanhMucChaId = null },
                new() { Id = khacId, TenDanhMuc = "Khác", Slug = "khac", IconUrl = "", ThuTu = 8, DanhMucChaId = null },

                // Danh mục con (ví dụ)
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111112"), DanhMucChaId = thucPhamId, TenDanhMuc = "Đồ khô", Slug = "do-kho", IconUrl = "", ThuTu = 1 },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), DanhMucChaId = doDungId, TenDanhMuc = "Đồ gia dụng", Slug = "do-gia-dung", IconUrl = "", ThuTu = 1 },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), DanhMucChaId = doDungId, TenDanhMuc = "Nội thất", Slug = "noi-that", IconUrl = "", ThuTu = 2 },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), DanhMucChaId = thoiTrangId, TenDanhMuc = "Áo quần", Slug = "ao-quan", IconUrl = "", ThuTu = 1 },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), DanhMucChaId = thoiTrangId, TenDanhMuc = "Giày dép", Slug = "giay-dep", IconUrl = "", ThuTu = 2 },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777778"), DanhMucChaId = doDienTuId, TenDanhMuc = "Điện thoại", Slug = "dien-thoai", IconUrl = "", ThuTu = 1 },
                new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777779"), DanhMucChaId = doDienTuId, TenDanhMuc = "Laptop", Slug = "laptop", IconUrl = "", ThuTu = 2 },
            };

            await db.DanhMucs.AddRangeAsync(data);
            await db.SaveChangesAsync();
        }

        private static async Task SeedTinhTrangAsync(AppDbContext db)
        {
            if (await db.TinhTrangSanPhams.AnyAsync()) return;

            var data = new List<TinhTrangSanPham>
            {
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                    TenTinhTrang = "Như mới (99%)",
                    DiemTinhTrang = 99,
                    MoTa = "Hầu như chưa sử dụng, gần như mới."
                },
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                    TenTinhTrang = "Mới (90%)",
                    DiemTinhTrang = 90,
                    MoTa = "Sử dụng ít, còn rất tốt."
                },
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                    TenTinhTrang = "Khá (70%)",
                    DiemTinhTrang = 70,
                    MoTa = "Có dấu hiệu sử dụng, hoạt động bình thường."
                },
                new()
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                    TenTinhTrang = "Cũ (50%)",
                    DiemTinhTrang = 50,
                    MoTa = "Đã dùng lâu, có thể có trầy xước/hao mòn."
                }
            };

            await db.TinhTrangSanPhams.AddRangeAsync(data);
            await db.SaveChangesAsync();
        }
    }
}

