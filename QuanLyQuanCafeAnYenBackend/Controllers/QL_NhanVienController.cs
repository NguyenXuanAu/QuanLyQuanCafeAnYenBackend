using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QL_NhanVienController : Controller
    {
        private readonly QuanLyQuanCafeDbContext _context;
        public QL_NhanVienController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVien>>> GetNhanViens(string? search)
        {
            var query = _context.NhanViens.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(n => n.HoTen.Contains(search) || n.MaNhanVien.Contains(search));
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NhanVien>> GetNhanVien(string id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound("Không tìm thấy nhân viên.");
            return nv;
        }

        [HttpPost]
        public async Task<ActionResult<NhanVien>> PostNhanVien(NhanVien nv)
        {
            if (_context.NhanViens.Any(e => e.MaNhanVien == nv.MaNhanVien))
            {
                return BadRequest("Mã nhân viên này đã tồn tại trong hệ thống.");
            }

            if (nv.NgayVaoLam == default || nv.NgayVaoLam == null)
            {
                nv.NgayVaoLam = DateOnly.FromDateTime(DateTime.Now);
            }

            _context.NhanViens.Add(nv);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNhanVien), new { id = nv.MaNhanVien }, nv);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutNhanVien(string id, NhanVien nv)
        {
            if (id != nv.MaNhanVien) return BadRequest("Mã nhân viên không khớp.");

            _context.Entry(nv).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.NhanViens.Any(e => e.MaNhanVien == id)) return NotFound();
                else throw;
            }

            return Ok("Cập nhật thông tin thành công.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNhanVien(string id)
        {
            var nv = await _context.NhanViens.FindAsync(id);
            if (nv == null) return NotFound("Nhân viên không tồn tại.");

            _context.NhanViens.Remove(nv);
            await _context.SaveChangesAsync();
            
            return Ok("Đã xóa nhân viên thành công.");
        }
    }
}
