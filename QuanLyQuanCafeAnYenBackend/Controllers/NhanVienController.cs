using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NhanVienController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public NhanVienController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // GET: api/NhanVien
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.NhanViens
                .Select(nv => new
                {
                    nv.MaNhanVien,
                    nv.HoTen
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
