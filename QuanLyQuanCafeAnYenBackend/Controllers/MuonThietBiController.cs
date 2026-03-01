using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MuonThietBiController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public MuonThietBiController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // =======================
        // GET: api/MuonThietBi
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.MuonThietBis
                .Include(x => x.MaNhanVienChoMuonNavigation)
                .Include(x => x.MaBanNavigation)
                .OrderByDescending(x => x.ThoiGianMuon)
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("device/{maNguoiDung}")]
        public async Task<IActionResult> GetDevice(string maNguoiDung)
        {
            var data = await _context.MuonThietBis
                .Include(x => x.MaNhanVienChoMuonNavigation)
                .Include(x => x.MaBanNavigation)
                .Where(x => x.MaNguoiDung == maNguoiDung)
                .Select(x => new
                {
                    x.MaLuotMuon,
                    x.TenThietBi,
                    x.TrangThai,
                    x.ThoiGianMuon,
                    x.ThoiGianTra,
                    x.MaBan,
                    TenBan = x.MaBanNavigation.TenBan,
                    x.MaNhanVienChoMuon,
                    TenNhanVien = x.MaNhanVienChoMuonNavigation.HoTen
                })
                .OrderByDescending(x => x.ThoiGianMuon)
                .ToListAsync();

            return Ok(data);
        }
        // =======================
        // GET: api/MuonThietBi/MT001
        // =======================
        [HttpGet("{maLuotMuon}")]
        public async Task<IActionResult> GetById(string maLuotMuon)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound("Không tìm thấy lượt mượn");

            return Ok(item);
        }

        // =======================
        // POST: api/MuonThietBi (MƯỢN THIẾT BỊ)
        // =======================
        [HttpPost]
        public async Task<IActionResult> MuonThietBi([FromBody] MuonThietBi model)
        {
            model.TrangThai = 0; // Đang xét duyệt
            model.ThoiGianMuon = null;
            model.ThoiGianTra = null;
            model.MaNhanVienChoMuon = null;

            _context.MuonThietBis.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }
        [HttpPut("Duyet/{maLuotMuon}")]
        public async Task<IActionResult> Duyet(string maLuotMuon, [FromBody] string maNhanVien)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound();

            item.TrangThai = 1; // Đang mượn
            item.MaNhanVienChoMuon = maNhanVien;
            item.ThoiGianMuon = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        [HttpPut("Tra/{maLuotMuon}")]
        public async Task<IActionResult> Tra(string maLuotMuon)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound();

            if (item.TrangThai != 1)
                return BadRequest("Không ở trạng thái đang mượn");

            item.TrangThai = 2; // Đã trả
            item.ThoiGianTra = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        [HttpPut("Update/{maLuotMuon}")]
        public async Task<IActionResult> Update(string maLuotMuon, [FromBody] MuonThietBi dto)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound();

            item.TenThietBi = dto.TenThietBi;
            item.MaBan = dto.MaBan?.Trim();
            item.MaNhanVienChoMuon = dto.MaNhanVienChoMuon?.Trim();
            item.MaNguoiDung = dto.MaNguoiDung?.Trim();

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var total = await _context.MuonThietBis.CountAsync();
            return Ok(total);
        }

        // =======================
        // DELETE: api/MuonThietBi/MT001
        // =======================
        [HttpDelete("{maLuotMuon}")]
        public async Task<IActionResult> Delete(string maLuotMuon)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound("Không tìm thấy lượt mượn");

            _context.MuonThietBis.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa lượt mượn");
        }
    }
}
