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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.ThoiGianMuon = DateTime.Now;
            model.TrangThai = 0; // Đang mượn
            model.ThoiGianTra = null;

            _context.MuonThietBis.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // =======================
        // PUT: api/MuonThietBi/TraThietBi/MT001
        // =======================
        [HttpPut("TraThietBi/{maLuotMuon}")]
        public async Task<IActionResult> TraThietBi(string maLuotMuon)
        {
            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound("Không tìm thấy lượt mượn");

            if (item.TrangThai == 1)
                return BadRequest("Thiết bị đã được trả");

            item.TrangThai = 1; // Đã trả
            item.ThoiGianTra = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(item);
        }
        [HttpPut("Update/{maLuotMuon}")]
        public async Task<IActionResult> Update(
    string maLuotMuon,
    [FromBody] MuonThietBi dto)
        {
            if (dto.MaLuotMuon == null)
                return BadRequest("Thiếu mã lượt mượn trong body");

            if (maLuotMuon != dto.MaLuotMuon)
                return BadRequest("Mã route và body không khớp");

            var item = await _context.MuonThietBis
                .FirstOrDefaultAsync(x => x.MaLuotMuon == maLuotMuon);

            if (item == null)
                return NotFound();

            item.TenThietBi = dto.TenThietBi;
            item.MaBan = dto.MaBan;
            item.MaNhanVienChoMuon = dto.MaNhanVienChoMuon;

            await _context.SaveChangesAsync();

            return Ok(item);
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
