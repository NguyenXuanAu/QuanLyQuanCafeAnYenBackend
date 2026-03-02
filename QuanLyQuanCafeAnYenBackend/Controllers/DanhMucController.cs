using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhMucController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public DanhMucController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }
        // GET: api/DanhMuc
        [HttpGet]
        public async Task<IActionResult> GetDanhMuc()
        {
            var data = await _context.DanhMucs
                .OrderBy(x => x.ThuTuHienThi)
                .ToListAsync();

            return Ok(data);
        }

    }
}
