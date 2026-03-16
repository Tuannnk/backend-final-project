using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bandothanhli.Areas.Admin.Models;
using bandothanhli.Data;
using bandothanhli.Models;

namespace bandothanhli.Areas.Admin.Controllers
{
    public class DanhMucController : AdminBaseController
    {
        private readonly AppDbContext _db;
        public DanhMucController(AppDbContext db) => _db = db;

        // GET /Admin/DanhMuc
        public async Task<IActionResult> Index()
        {
            var list = await _db.DanhMucs
                .Include(d => d.DanhMucCha)
                .Select(d => new DanhMucVM
                {
                    Id = d.Id,
                    TenDanhMuc = d.TenDanhMuc,
                    Slug = d.Slug,
                    IconUrl = d.IconUrl,
                    ThuTu = d.ThuTu,
                    DanhMucChaId = d.DanhMucChaId,
                    TenDanhMucCha = d.DanhMucCha != null ? d.DanhMucCha.TenDanhMuc : null,
                    SoSanPham = d.SanPhams.Count
                })
                .OrderBy(d => d.ThuTu)
                .ToListAsync();

            return View(list);
        }

        // GET /Admin/DanhMuc/Them
        public async Task<IActionResult> Them()
        {
            ViewBag.DanhSachCha = await _db.DanhMucs
                .OrderBy(d => d.TenDanhMuc)
                .ToListAsync();

            return View(new DanhMucVM());
        }

        // GET /Admin/DanhMuc/Sua/{id}
        public async Task<IActionResult> Sua(Guid id)
        {
            var d = await _db.DanhMucs.FindAsync(id);
            if (d == null) return NotFound();

            ViewBag.DanhSachCha = await _db.DanhMucs
                .Where(x => x.Id != id)
                .OrderBy(x => x.TenDanhMuc)
                .ToListAsync();

            return View(new DanhMucVM
            {
                Id = d.Id,
                TenDanhMuc = d.TenDanhMuc,
                Slug = d.Slug,
                IconUrl = d.IconUrl,
                ThuTu = d.ThuTu,
                DanhMucChaId = d.DanhMucChaId
            });
        }

        // POST /Admin/DanhMuc/Luu  — dùng chung cho cả Thêm lẫn Sửa
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Luu(DanhMucVM model)
        {
            // Nạp lại ViewBag nếu cần trả về view
            ViewBag.DanhSachCha = await _db.DanhMucs
                .Where(x => x.Id != model.Id)
                .OrderBy(x => x.TenDanhMuc)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model.Id == Guid.Empty ? "Them" : "Sua", model);

            // Kiểm tra trùng tên
            bool trungTen = await _db.DanhMucs
                .AnyAsync(d => d.TenDanhMuc == model.TenDanhMuc
                            && d.Id != model.Id);
            if (trungTen)
            {
                ModelState.AddModelError("TenDanhMuc", "Tên danh mục đã tồn tại");
                return View(model.Id == Guid.Empty ? "Them" : "Sua", model);
            }

            // Tạo slug nếu để trống
            var slug = string.IsNullOrWhiteSpace(model.Slug)
                ? TaoSlug(model.TenDanhMuc)
                : model.Slug.Trim();

            if (model.Id == Guid.Empty)
            {
                // ── THÊM MỚI ──
                _db.DanhMucs.Add(new DanhMuc
                {
                    Id = Guid.NewGuid(),
                    TenDanhMuc = model.TenDanhMuc.Trim(),
                    Slug = slug,
                    IconUrl = model.IconUrl?.Trim() ?? "",
                    ThuTu = model.ThuTu,
                    DanhMucChaId = model.DanhMucChaId
                });
                TempData["Success"] = $"Đã thêm danh mục '{model.TenDanhMuc}'";
            }
            else
            {
                // ── CẬP NHẬT ──
                var d = await _db.DanhMucs.FindAsync(model.Id);
                if (d == null) return NotFound();

                d.TenDanhMuc = model.TenDanhMuc.Trim();
                d.Slug = slug;
                d.IconUrl = model.IconUrl?.Trim() ?? "";
                d.ThuTu = model.ThuTu;
                d.DanhMucChaId = model.DanhMucChaId;
                TempData["Success"] = $"Đã cập nhật danh mục '{model.TenDanhMuc}'";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // POST /Admin/DanhMuc/Xoa/{id}
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var d = await _db.DanhMucs.FindAsync(id);
            if (d == null) return NotFound();

            // Kiểm tra còn sản phẩm
            bool coSanPham = await _db.SanPhams.AnyAsync(s => s.DanhMucId == id);
            if (coSanPham)
            {
                TempData["Error"] = "Không thể xóa! Danh mục đang có sản phẩm.";
                return RedirectToAction("Index");
            }

            // Kiểm tra còn danh mục con
            bool coCon = await _db.DanhMucs.AnyAsync(x => x.DanhMucChaId == id);
            if (coCon)
            {
                TempData["Error"] = "Không thể xóa! Danh mục đang có danh mục con.";
                return RedirectToAction("Index");
            }

            _db.DanhMucs.Remove(d);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã xóa danh mục '{d.TenDanhMuc}'";
            return RedirectToAction("Index");
        }

        // Helper tạo slug
        private static string TaoSlug(string ten)
        {
            return ten.ToLower()
                .Normalize(System.Text.NormalizationForm.FormD)
                .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                            != System.Globalization.UnicodeCategory.NonSpacingMark)
                .Aggregate("", (s, c) => s + c)
                .Replace("đ", "d")
                .Replace(" ", "-")
                .Replace("--", "-")
                .Trim('-');
        }
    }
}