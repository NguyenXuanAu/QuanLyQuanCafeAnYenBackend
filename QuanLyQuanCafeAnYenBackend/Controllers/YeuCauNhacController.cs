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
        public async Task<IActionResult> Update(string maYeuCau, [FromBody] YeuCauNhac model)
        {
            if (maYeuCau != model.MaYeuCau)
                return BadRequest("Mã yêu cầu không khớp");

            var exists = await _context.YeuCauNhacs.AnyAsync(x => x.MaYeuCau == maYeuCau);
            if (!exists)
                return NotFound("Không tìm thấy yêu cầu nhạc");

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(model);
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
        [HttpPost("BinhChon/{maYeuCau}")]
        public async Task<IActionResult> BinhChon(string maYeuCau)
        {
            var item = await _context.YeuCauNhacs
                .FirstOrDefaultAsync(x => x.MaYeuCau == maYeuCau);

            if (item == null)
                return NotFound("Không tìm thấy yêu cầu nhạc");

            item.SoLuotBinhChon += 1;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                item.MaYeuCau,
                item.SoLuotBinhChon
            });
        }
    }
}
