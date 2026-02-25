using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

[Route("api/[controller]")]
[ApiController]
public class MonAnController : ControllerBase
{
    private readonly QuanLyQuanCafeDbContext _context;

    public MonAnController(QuanLyQuanCafeDbContext context)
    {
        _context = context;
    }

    // ================= GET =================
    // GET: api/MonAn
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _context.MonAns
            .Include(x => x.MaDanhMucNavigation)
            .Select(x => new
            {
                x.MaMonAn,
                x.TenMonAn,
                x.GiaTien,
                x.MaDanhMuc,
                TenDanhMuc = x.MaDanhMucNavigation.TenDanhMuc,
                x.DonViTinh,
                x.ConHang,
                x.ChuThich,
                x.MoTa
            })
            .ToListAsync();

        return Ok(data);
    }

    // ================= POST =================
    // POST: api/MonAn
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MonAnDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MaMonAn))
            return BadRequest("Mã món không được để trống");

        dto.MaMonAn = dto.MaMonAn.Trim();

        if (dto.MaMonAn.Length > 10)
            return BadRequest("Mã món tối đa 10 ký tự");

        if (await _context.MonAns.AnyAsync(x => x.MaMonAn == dto.MaMonAn))
            return BadRequest("Mã món đã tồn tại");

        if (!await _context.DanhMucs.AnyAsync(x => x.MaDanhMuc == dto.MaDanhMuc))
            return BadRequest("Danh mục không tồn tại");

        var mon = new MonAn
        {
            MaMonAn = dto.MaMonAn,
            TenMonAn = dto.TenMonAn.Trim(),
            GiaTien = dto.GiaTien,
            MaDanhMuc = dto.MaDanhMuc.Trim(),
            DonViTinh = dto.DonViTinh ?? "Ly",
            ConHang = dto.ConHang ?? true,
            ChuThich = dto.ChuThich,
            MoTa = dto.MoTa ?? ""
        };

        _context.MonAns.Add(mon);
        await _context.SaveChangesAsync();

        return Ok(mon);
    }

    // ================= PUT =================
    // PUT: api/MonAn/MA001
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] MonAnDto dto)
    {
        id = id.Trim();

        var mon = await _context.MonAns
            .FirstOrDefaultAsync(x => x.MaMonAn.Trim() == id);

        if (mon == null)
            return BadRequest("Món ăn không tồn tại");

        if (!await _context.DanhMucs.AnyAsync(x => x.MaDanhMuc == dto.MaDanhMuc))
            return BadRequest("Danh mục không tồn tại");

        mon.TenMonAn = dto.TenMonAn.Trim();
        mon.GiaTien = dto.GiaTien;
        mon.MaDanhMuc = dto.MaDanhMuc.Trim();
        mon.DonViTinh = dto.DonViTinh ?? "Ly";
        mon.ConHang = dto.ConHang ?? true;
        mon.ChuThich = dto.ChuThich;
        mon.MoTa = dto.MoTa ?? "";

        await _context.SaveChangesAsync();
        return Ok("Sửa thành công");
    }

    // ================= DELETE =================
    // DELETE: api/MonAn/MA001
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var mon = await _context.MonAns.FindAsync(id.Trim());
        if (mon == null)
            return NotFound("Món ăn không tồn tại");

        _context.MonAns.Remove(mon);
        await _context.SaveChangesAsync();

        return Ok("Xóa thành công");
    }
}
