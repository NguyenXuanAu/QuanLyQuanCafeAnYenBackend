using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonAnHomeController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public MonAnHomeController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET: api/MonAn
        // GET: api/MonAn?maDanhMuc=CF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? maDanhMuc)
        {
            var query = _context.MonAns
                .Where(x => x.ConHang == true);

            if (!string.IsNullOrEmpty(maDanhMuc))
            {
                query = query.Where(x => x.MaDanhMuc == maDanhMuc);
            }

            var data = await query
                .Select(x => new
                {
                    x.MaMonAn,
                    x.TenMonAn,
                    x.GiaTien,
                    x.DonViTinh,
                    TenDanhMuc = x.MaDanhMucNavigation.TenDanhMuc,
                    AnhDaiDien = x.HinhAnhMonAns
                        .Where(h => h.LaAnhDaiDien == true)
                        .Select(h => h.DuongDanAnh)
                        .FirstOrDefault()
                })
                .Take(12) // chỉ lấy tối đa 12 món
                .ToListAsync();

            return Ok(data);
        }

        // =========================================
        // GET: api/MonAn/MA001
        // =========================================
        [HttpGet("{maMonAn}")]
        public async Task<IActionResult> GetById(string maMonAn)
        {
            var item = await _context.MonAns
                .Where(x => x.MaMonAn == maMonAn)
                .Select(x => new
                {
                    x.MaMonAn,
                    x.TenMonAn,
                    x.GiaTien,
                    x.MoTa,
                    x.DonViTinh,
                    x.ConHang,
                    DanhMuc = x.MaDanhMucNavigation.TenDanhMuc,
                    HinhAnh = x.HinhAnhMonAns
                        .OrderBy(h => h.ThuTuHienThi)
                        .Select(h => h.DuongDanAnh)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (item == null)
                return NotFound("Không tìm thấy món ăn");

            return Ok(item);
        }
    }
}
