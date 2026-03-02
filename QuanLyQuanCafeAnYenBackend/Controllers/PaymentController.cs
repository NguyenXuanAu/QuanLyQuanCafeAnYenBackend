using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public PaymentController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPayments()
        {
            var data = await _context.DonDatBans
                .Select(d => new
                {
                    maDonDat = d.MaDonDat,
                    tenKhach = d.TenKhachHang, // thêm nếu có
                    tongTien = d.TongTienDatTruoc,
                    soTienDaCoc = d.TienCoc,
                    trangThai = d.TrangThai == 1
                        ? "Đã thanh toán"
                        : d.TrangThai == 2
                            ? "Đã cọc"
                            : "Chưa thanh toán"
                })
                .ToListAsync();

            return Ok(data);
        }
        [HttpPut("MarkAsPaid/{maDonDat}")]
        public async Task<IActionResult> MarkAsPaid(string maDonDat)
        {
            var hoaDon = await _context.DonDatBans
                .FirstOrDefaultAsync(h => h.MaDonDat == maDonDat);

            if (hoaDon == null)
                return NotFound("Không tìm thấy hóa đơn");

            hoaDon.TrangThai = 1; // 1 = Đã thanh toán

            await _context.SaveChangesAsync();

            return Ok("Cập nhật thành công");
        }
    }
}