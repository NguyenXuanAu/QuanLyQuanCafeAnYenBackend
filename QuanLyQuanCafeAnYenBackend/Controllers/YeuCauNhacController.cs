using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YeuCauNhacController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public YeuCauNhacController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }
        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var total = await _context.YeuCauNhacs.CountAsync();
            return Ok(total);
        }
        // =======================
        // GET: api/YeuCauNhac
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.YeuCauNhacs
                .OrderByDescending(x => x.SoLuotBinhChon)
                .ThenBy(x => x.ThoiGianYeuCau)
                .ToListAsync();

            return Ok(data);
        }
        [HttpGet("list/{maNguoiDung}")]
        public async Task<IActionResult> GetList(string maNguoiDung)
        {
            var data = await _context.YeuCauNhacs
                .Include(x => x.MaBanNavigation)
                .Select(n => new
                {
                    n.MaYeuCau,
                    n.TenBaiHat,
                    n.CaSi,
                    n.SoLuotBinhChon,
                    n.DaPhat,
                    n.MaBan,
                    TenBan = n.MaBanNavigation.TenBan,
                    n.NguoiYeuCau,
                    n.MaNguoiDung,

                    DaVote = _context.NhacVote
                        .Any(v => v.MaYeuCau == n.MaYeuCau
                               && v.MaNguoiDung == maNguoiDung)
                })
                .OrderByDescending(x => x.SoLuotBinhChon)
                .ToListAsync();

            return Ok(data);
        }
        // =======================
        // GET: api/YeuCauNhac/YC001
        // =======================
        [HttpGet("{maYeuCau}")]
        public async Task<IActionResult> GetById(string maYeuCau)
        {
            var item = await _context.YeuCauNhacs
                .FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau);

            if (item == null)
                return NotFound("Không tìm thấy yêu cầu nhạc");

            return Ok(item);
        }

        // =======================
        // POST: api/YeuCauNhac
        // =======================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] YeuCauNhac model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Nếu frontend chưa truyền SoLuotBinhChon
            model.SoLuotBinhChon ??= 1;
            model.DaPhat ??= false;
            model.ThoiGianYeuCau = DateTime.Now;

            _context.YeuCauNhacs.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        // =======================
        // PUT: api/YeuCauNhac/YC001
        // =======================
        [HttpPut("{maYeuCau}")]
        public async Task<IActionResult> Update(string maYeuCau, [FromBody] YeuCauNhac dto)
        {
            var item = await _context.YeuCauNhacs
                .FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau);

            if (item == null)
                return NotFound();

            item.TenBaiHat = dto.TenBaiHat;
            item.CaSi = dto.CaSi;
            item.LinkVideo = dto.LinkVideo;
            item.MaBan = dto.MaBan;
            item.NguoiYeuCau = dto.NguoiYeuCau;
            item.DaPhat = dto.DaPhat;

            await _context.SaveChangesAsync();

            return Ok(item);
        }

        // =======================
        // DELETE: api/YeuCauNhac/YC001
        // =======================
        [HttpDelete("{maYeuCau}")]
        public async Task<IActionResult> Delete(string maYeuCau)
        {
            var item = await _context.YeuCauNhacs
                .FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau);

            if (item == null)
                return NotFound("Không tìm thấy yêu cầu nhạc");

            _context.YeuCauNhacs.Remove(item);
            await _context.SaveChangesAsync();

            return Ok("Đã xóa thành công");
        }

        // =======================
        // POST: api/YeuCauNhac/BinhChon/YC001
        // =======================
        
    }
}
