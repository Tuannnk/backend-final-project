using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;
using bandothanhli.Models;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class TinhTrangController : AdminBaseController
    {
        private readonly AppDbContext _db;
        public TinhTrangController(AppDbContext db) => _db = db;

        // GET /Admin/TinhTrang
        public async Task<IActionResult> Index()
        {
            var list = await _db.TinhTrangSanPhams
                .Select(t => new TinhTrangVM
                {
                    Id = t.Id,
                    TenTinhTrang = t.TenTinhTrang,
                    DiemTinhTrang = t.DiemTinhTrang,
                    MoTa = t.MoTa,
                    SoSanPham = t.SanPhams.Count
                })
                .OrderByDescending(t => t.DiemTinhTrang)
                .ToListAsync();

            return View(list);
        }

        // GET /Admin/TinhTrang/Them
        public IActionResult Them()
        {
            return View(new TinhTrangVM());
        }

        // GET /Admin/TinhTrang/Sua/{id}
        public async Task<IActionResult> Sua(Guid id)
        {
            var t = await _db.TinhTrangSanPhams.FindAsync(id);
            if (t == null) return NotFound();

            return View(new TinhTrangVM
            {
                Id = t.Id,
                TenTinhTrang = t.TenTinhTrang,
                DiemTinhTrang = t.DiemTinhTrang,
                MoTa = t.MoTa
            });
        }

        // POST /Admin/TinhTrang/Luu — dùng chung cho cả Thêm lẫn Sửa
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(TinhTrangVM model)
        {
            if (!ModelState.IsValid)
                return View(model.Id == Guid.Empty ? "Them" : "Sua", model);

            // Kiểm tra trùng tên
            bool trungTen = await _db.TinhTrangSanPhams
                .AnyAsync(t => t.TenTinhTrang == model.TenTinhTrang
                            && t.Id != model.Id);
            if (trungTen)
            {
                ModelState.AddModelError("TenTinhTrang", "Tên tình trạng đã tồn tại");
                return View(model.Id == Guid.Empty ? "Them" : "Sua", model);
            }

            if (model.Id == Guid.Empty)
            {
                // ── THÊM MỚI ──
                _db.TinhTrangSanPhams.Add(new TinhTrangSanPham
                {
                    Id = Guid.NewGuid(),
                    TenTinhTrang = model.TenTinhTrang.Trim(),
                    DiemTinhTrang = model.DiemTinhTrang,
                    MoTa = model.MoTa?.Trim() ?? ""
                });
                TempData["Success"] = $"Đã thêm tình trạng '{model.TenTinhTrang}'";
            }
            else
            {
                // ── CẬP NHẬT ──
                var t = await _db.TinhTrangSanPhams.FindAsync(model.Id);
                if (t == null) return NotFound();

                t.TenTinhTrang = model.TenTinhTrang.Trim();
                t.DiemTinhTrang = model.DiemTinhTrang;
                t.MoTa = model.MoTa?.Trim() ?? "";
                TempData["Success"] = $"Đã cập nhật tình trạng '{model.TenTinhTrang}'";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST /Admin/TinhTrang/Xoa/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var t = await _db.TinhTrangSanPhams.FindAsync(id);
            if (t == null) return NotFound();

            // Kiểm tra còn sản phẩm đang dùng
            bool coSanPham = await _db.SanPhams.AnyAsync(s => s.TinhTrangId == id);
            if (coSanPham)
            {
                TempData["Error"] = "Không thể xóa! Tình trạng đang được dùng cho sản phẩm.";
                return RedirectToAction("Index");
            }

            _db.TinhTrangSanPhams.Remove(t);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa tình trạng '{t.TenTinhTrang}'";
            return RedirectToAction("Index");
        }
    }
}