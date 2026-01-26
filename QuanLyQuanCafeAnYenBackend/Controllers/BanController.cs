using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BanController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public BanController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // GET: api/Ban
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.Bans
                .Select(b => new
                {
                    b.MaBan,
                    b.TenBan
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}
