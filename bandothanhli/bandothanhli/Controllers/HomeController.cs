using bandothanhli.Data;
using bandothanhli.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace bandothanhli.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(AppDbContext db, ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _db = db;
            _logger = logger;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy sản phẩm mới nhất (bỏ qua filter TrangThai để test)
                var sanPhamsMoi = await _db.SanPhams
                    .Include(s => s.AnhSanPhams)
                    .OrderByDescending(s => s.NgayTao)
                    .Take(6)
                    .ToListAsync();

                // Debug: Log để kiểm tra
                _logger.LogInformation($"=== HOME INDEX DEBUG ===");
                _logger.LogInformation($"WebRootPath: {_env.WebRootPath}");
                _logger.LogInformation($"Total products: {sanPhamsMoi.Count}");
                
                foreach (var sp in sanPhamsMoi)
                {
                    _logger.LogInformation($"\nProduct: {sp.TieuDe}");
                    _logger.LogInformation($"  Status: {sp.TrangThai}");
                    _logger.LogInformation($"  Images Count: {sp.AnhSanPhams?.Count ?? 0}");
                    
                    if (sp.AnhSanPhams?.Any() == true)
                    {
                        foreach (var anh in sp.AnhSanPhams)
                        {
                            var filePath = Path.Combine(_env.WebRootPath, anh.DuongDanAnh.TrimStart('/'));
                            var exists = System.IO.File.Exists(filePath);
                            _logger.LogInformation($"    - [{anh.LaAnhDaiDien}] {anh.DuongDanAnh} (File exists: {exists})");
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"    - No images");
                    }
                }

                return View(sanPhamsMoi);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi Index: {ex.Message} \n {ex.StackTrace}");
                return View(new List<SanPham>());
            }
        }

        // Debug endpoint để kiểm tra folder images
        [Route("debug/images")]
        public IActionResult DebugImages()
        {
            try
            {
                var imagesPath = Path.Combine(_env.WebRootPath, "images");
                var files = new List<object>();

                if (Directory.Exists(imagesPath))
                {
                    var fileList = Directory.GetFiles(imagesPath);
                    foreach (var file in fileList)
                    {
                        var fileInfo = new FileInfo(file);
                        files.Add(new
                        {
                            Name = fileInfo.Name,
                            Size = fileInfo.Length,
                            Path = $"/images/{fileInfo.Name}"
                        });
                    }
                }

                var result = new
                {
                    WebRootPath = _env.WebRootPath,
                    ImagesPath = imagesPath,
                    DirectoryExists = Directory.Exists(imagesPath),
                    FileCount = files.Count,
                    Files = files
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
