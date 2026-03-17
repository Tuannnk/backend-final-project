# backend-final-project
đây là dự án bài tập lớn môn Backend
I. Fix lỗi không hiển thị ảnh sản phẩm:
  1. Khởi tạo Collections trong Model:
// bandothanhli/Models/SanPham.cs
public SanPham()
{
    AnhSanPhams = new List<AnhSanPham>();
    ChiTietDonHangs = new List<ChiTietDonHang>();
    DanhGias = new List<DanhGia>();
}
Lý do: Tránh NullReferenceException khi truy cập AnhSanPhams
  2. Tạo ImageHelper để xử lý đường dẫn:
// bandothanhli/Helpers/ImageHelper.cs
public static string GetImagePath(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
        return "/images/picture1.png";
    
    var trimmed = path.Trim().Replace("\\", "/");
    if (!trimmed.StartsWith("/"))
        trimmed = "/" + trimmed;
    
    return trimmed;
}
Lý do: Normalize đường dẫn ảnh (loại bỏ backslash, khoảng trắng, đảm bảo / đầu)
  3. Cập nhật View để sử dụng ImageHelper:
@using bandothanhli.Helpers
<!-- ... -->
@{
    var anhDaiDien = sp.AnhSanPhams?
        .FirstOrDefault(a => a.LaAnhDaiDien)?.DuongDanAnh
        ?? sp.AnhSanPhams?.FirstOrDefault()?.DuongDanAnh;
    
    var imagePath = ImageHelper.GetImagePath(anhDaiDien);
}
<img src="@imagePath" onerror="this.src='/images/picture1.png';" />
Lý do: Xử lý đường dẫn + fallback ảnh mặc định nếu load fail
  4. Cập nhật Program.cs - Enable Static Files
app.UseStaticFiles();  // Phục vụ files từ wwwroot/
Lý do: Cho phép ASP.NET Core phục vụ ảnh từ /images/
  5. Tạo DataSeederService - Auto Insert Data
// bandothanhli/Services/DataSeederService.cs
public async Task SeedDataAsync()
{
    // Insert 3 sản phẩm test với ảnh
    for (int i = 1; i <= 3; i++)
    {
        var product = new SanPham { /* ... */ };
        var image = new AnhSanPham
        {
            DuongDanAnh = i % 2 == 1 ? "/images/picture1.png" : "/images/picture2.png",
            LaAnhDaiDien = true
        };
    }
}
Lý do: Tự động tạo dữ liệu test khi app khởi động
  6. Đăng ký Service và Gọi Seed
// Program.cs
builder.Services.AddScoped<IDataSeederService, DataSeederService>();

// Khởi động app
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
    await seeder.SeedDataAsync();
}
Lý do: Service tự động chạy lúc app start, insert dữ liệu
  7. Cập nhật HomeController
var sanPhamsMoi = await _db.SanPhams
    .Include(s => s.AnhSanPhams)
    .OrderByDescending(s => s.NgayTao)
    .Take(6)
    .ToListAsync();
Lý do: Include ảnh + order theo ngày mới nhất
  * Lưu ý quan trọng
1.	Folder wwwroot/images/ phải chứa:
•	picture1.png (file ảnh thực)
•	picture2.png (file ảnh thực)
2.	Nếu muốn thêm ảnh mới: Upload qua API /api/san-pham/upload-anh
3.	Nếu muốn loại bỏ seed data: Chỉ cần xóa dòng gọi seeder.SeedDataAsync() trong Program.cs

II. Vấn đề lưu đăng nhập:
  1. Nguyên nhân:
    - Tôi thấy vấn đề rồi! Bạn đang sử dụng JWT Authentication nhưng User.Identity.IsAuthenticated chỉ hoạt động khi JWT token được gửi trong request. Vấn đề là        sau khi đăng nhập, token không được lưu ở browser và gửi lại trong các request tiếp theo.
    - AuthController trả về JWT token nhưng client-side (view) không lưu nó.
  2. Giải pháp:
    - Sử dụng HTTP-only Cookies để browser tự động gửi token
    - Sử dụng HTTP-only Cookies để browser tự động gửi token
    - thêm endpoint Logout để xóa cookie
    - cập nhật form Logout trong Layout để gọi đúng API endpoint
    - thêm JavaScript để handle logout
