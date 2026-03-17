using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;

namespace bandothanhli.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db) => _db = db;

        [HttpGet]
        public IActionResult DangNhap()
        {
            if (User.Identity!.IsAuthenticated && User.IsInRole("Admin"))
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapVM model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Email = model.Email.Trim();

            var nd = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (nd == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View(model);
            }

            if (nd.VaiTro != "Admin")
            {
                ModelState.AddModelError("", "Tài khoản này không có quyền truy cập trang quản trị.");
                return View(model);
            }

            if (!nd.DaXacThuc)
            {
                ModelState.AddModelError("", "Tài khoản chưa được kích hoạt.");
                return View(model);
            }

            if (!Verify(model.MatKhau, nd.MatKhau))
            {
                ModelState.AddModelError("", "Mật khẩu không đúng.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, nd.Id.ToString()),
                new(ClaimTypes.Name,  nd.HoTen),
                new(ClaimTypes.Email, nd.Email),
                new(ClaimTypes.Role,  "Admin")
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims,
                    CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true });

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("DangNhap");
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public IActionResult DoiMatKhau() => View();

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var nd = await _db.NguoiDungs.FindAsync(id);
            if (nd == null) return NotFound();

            if (!Verify(model.MatKhauCu, nd.MatKhau))
            {
                ModelState.AddModelError("MatKhauCu", "Mật khẩu cũ không đúng");
                return View(model);
            }

            nd.MatKhau = Hash(model.MatKhauMoi);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        static string Hash(string s) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
        static bool Verify(string s, string h) => Hash(s) == h;
    }
}
