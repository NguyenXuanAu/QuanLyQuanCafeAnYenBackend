using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        public ThongKeController(QuanLyQuanCafeDbContext context) => _context = context;

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] DateTime tuNgay, [FromQuery] DateTime denNgay)
        {
            try
            {
                var start = tuNgay.Date;
                var end = denNgay.Date.AddDays(1);

                
                var hoaDons = await _context.HoaDons
                    .Where(h => h.ThoiGianVao >= start && h.ThoiGianVao < end)
                    .ToListAsync();

                var tongDoanhThu = hoaDons.Sum(h => h.TongTienThanhToan);
                var tongDonHang = hoaDons.Count;
                var khachHangUnique = hoaDons.Select(h => h.MaNguoiDung).Distinct().Count();
                var donTrungBinh = tongDonHang > 0 ? tongDoanhThu / tongDonHang : 0;

                // Thống kê Top 10 
                var topSanPham = await _context.ChiTietHoaDons
                    .Include(ct => ct.MonAn)
                    .Where(ct => ct.HoaDon.ThoiGianVao >= start && ct.HoaDon.ThoiGianVao < end)
                    .Select(ct => new {
                        TenMon = ct.MonAn != null ? ct.MonAn.TenMonAn : "Món không xác định",
                        ct.SoLuong,
                        ct.ThanhTien
                    })
                    .ToListAsync();

                var groupedTop = topSanPham
                    .GroupBy(x => x.TenMon)
                    .Select(g => new {
                        TenSanPham = g.Key,
                        SoLuong = g.Sum(x => x.SoLuong),
                        DoanhThu = g.Sum(x => x.ThanhTien)
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .Take(10)
                    .ToList();
                // Thống kê doanh thu theo ngày
                var doanhThuTheoNgay = hoaDons
                     .GroupBy(h => h.ThoiGianVao.Value.Date)
                     .Select(g => new {
                     Ngay = g.Key.ToString("yyyy-MM-dd"), 
                     DoanhThu = g.Sum(x => x.TongTienThanhToan) })
                         .OrderBy(x => x.Ngay)
                         .ToList();


                return Ok(new
                {
                    TongDoanhThu = tongDoanhThu,
                    TongDonHang = tongDonHang,
                    KhachHang = khachHangUnique,
                    DonTrungBinh = donTrungBinh,
                    TopSanPham = groupedTop,
                    DoanhThuTheoNgay = doanhThuTheoNgay
                });


            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); 
            }

        }
    }
}
