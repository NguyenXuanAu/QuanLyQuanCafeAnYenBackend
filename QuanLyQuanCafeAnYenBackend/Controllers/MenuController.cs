using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

[Route("api/[controller]")]
[ApiController]
public class MenuController : ControllerBase
{
    private readonly QuanLyQuanCafeDbContext _context;

    public MenuController(QuanLyQuanCafeDbContext context)
    {
        _context = context;
    }

    // GET: api/Menu
    [HttpGet]
    public IActionResult GetAll()
    {
        var data = _context.MonAns
            .Include(x => x.MaDanhMucNavigation)
            .Select(x => new
            {
                x.MaMonAn,
                x.TenMonAn,
                x.GiaTien,
                x.ConHang,
                x.DonViTinh,
                x.MaDanhMuc,
                TenDanhMuc = x.MaDanhMucNavigation.TenDanhMuc
            })
            .OrderBy(x => x.MaDanhMuc)
            .ToList();

        return Ok(data);
    }

    // GET: api/Menu/byDanhMuc/DM01
    [HttpGet("byDanhMuc/{maDanhMuc}")]
    public IActionResult GetByDanhMuc(string maDanhMuc)
    {
        var data = _context.MonAns
            .Where(x => x.MaDanhMuc == maDanhMuc)
            .Include(x => x.MaDanhMucNavigation)
            .Select(x => new
            {
                x.MaMonAn,
                x.TenMonAn,
                x.GiaTien,
                x.ConHang,
                TenDanhMuc = x.MaDanhMucNavigation.TenDanhMuc
            })
            .ToList();

        return Ok(data);
    }
}
