using bandothanhli.Data;
using bandothanhli.DTOs;
using bandothanhli.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace bandothanhli.Controllers
{
    [ApiController]
    [Route("api/dia-chi")]
    [Authorize]
    public class DiaChiController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DiaChiController(AppDbContext db)
        {
            _db = db;
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET /api/dia-chi
        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            var userId = GetCurrentUserId();
            var data = await _db.DiaChis
                .Where(d => d.NguoiDungId == userId)
                .OrderByDescending(d => d.LaMacDinh)
                .Select(d => new
                {
                    d.Id,
                    d.HoTen,
                    d.SoDienThoai,
                    d.DuongPho,
                    d.QuanHuyen,
                    d.TinhThanh,
                    d.LaMacDinh
                })
                .ToListAsync();

            return Ok(data);
        }

        // POST /api/dia-chi
        [HttpPost]
        public async Task<IActionResult> Them([FromBody] DiaChiDTO dto)
        {
            var userId = GetCurrentUserId();

            // Nếu đặt làm mặc định thì bỏ mặc định của địa chỉ cũ
            if (dto.LaMacDinh)
            {
                var diaChiCu = await _db.DiaChis
                    .Where(d => d.NguoiDungId == userId && d.LaMacDinh)
                    .ToListAsync();
                diaChiCu.ForEach(d => d.LaMacDinh = false);
            }

            // Nếu chưa có địa chỉ nào thì tự động đặt làm mặc định
            var chuaCodiaChi = !await _db.DiaChis.AnyAsync(d => d.NguoiDungId == userId);

            var diaChi = new DiaChi
            {
                Id = Guid.NewGuid(),
                NguoiDungId = userId,
                HoTen = dto.HoTen,
                SoDienThoai = dto.SoDienThoai,
                DuongPho = dto.DuongPho,
                QuanHuyen = dto.QuanHuyen,
                TinhThanh = dto.TinhThanh,
                LaMacDinh = dto.LaMacDinh || chuaCodiaChi
            };

            await _db.DiaChis.AddAsync(diaChi);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Thêm địa chỉ thành công", id = diaChi.Id });
        }

        // PUT /api/dia-chi/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> CapNhat(Guid id, [FromBody] DiaChiDTO dto)
        {
            var userId = GetCurrentUserId();
            var diaChi = await _db.DiaChis.FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == userId);

            if (diaChi == null) return NotFound(new { message = "Không tìm thấy địa chỉ" });

            // Nếu đặt làm mặc định thì bỏ mặc định của địa chỉ cũ
            if (dto.LaMacDinh && !diaChi.LaMacDinh)
            {
                var diaChiCu = await _db.DiaChis
                    .Where(d => d.NguoiDungId == userId && d.LaMacDinh)
                    .ToListAsync();
                diaChiCu.ForEach(d => d.LaMacDinh = false);
            }

            diaChi.HoTen = dto.HoTen ?? diaChi.HoTen;
            diaChi.SoDienThoai = dto.SoDienThoai ?? diaChi.SoDienThoai;
            diaChi.DuongPho = dto.DuongPho ?? diaChi.DuongPho;
            diaChi.QuanHuyen = dto.QuanHuyen ?? diaChi.QuanHuyen;
            diaChi.TinhThanh = dto.TinhThanh ?? diaChi.TinhThanh;
            diaChi.LaMacDinh = dto.LaMacDinh;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Cập nhật địa chỉ thành công" });
        }

        // DELETE /api/dia-chi/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Xoa(Guid id)
        {
            var userId = GetCurrentUserId();
            var diaChi = await _db.DiaChis.FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == userId);

            if (diaChi == null) return NotFound(new { message = "Không tìm thấy địa chỉ" });

            // Không cho xóa địa chỉ đang dùng trong đơn hàng
            var dangDung = await _db.DonHangs.AnyAsync(d => d.DiaChiId == id);
            if (dangDung) return BadRequest(new { message = "Địa chỉ này đang được dùng trong đơn hàng, không thể xóa" });

            _db.DiaChis.Remove(diaChi);
            await _db.SaveChangesAsync();

            // Nếu xóa địa chỉ mặc định thì tự động đặt địa chỉ đầu tiên còn lại làm mặc định
            if (diaChi.LaMacDinh)
            {
                var diaChiDauTien = await _db.DiaChis.FirstOrDefaultAsync(d => d.NguoiDungId == userId);
                if (diaChiDauTien != null)
                {
                    diaChiDauTien.LaMacDinh = true;
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Xóa địa chỉ thành công" });
        }

        // PUT /api/dia-chi/{id}/mac-dinh
        [HttpPut("{id}/mac-dinh")]
        public async Task<IActionResult> DatMacDinh(Guid id)
        {
            var userId = GetCurrentUserId();
            var diaChi = await _db.DiaChis.FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == userId);

            if (diaChi == null) return NotFound(new { message = "Không tìm thấy địa chỉ" });

            // Bỏ mặc định tất cả địa chỉ cũ
            var tatCaDiaChi = await _db.DiaChis.Where(d => d.NguoiDungId == userId).ToListAsync();
            tatCaDiaChi.ForEach(d => d.LaMacDinh = false);

            // Đặt địa chỉ này làm mặc định
            diaChi.LaMacDinh = true;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã đặt làm địa chỉ mặc định" });
        }
    }
}