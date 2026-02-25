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

        [HttpGet("thongke")]
        public async Task<IActionResult> GetDashboardData(DateTime tuNgay, DateTime denNgay)
        {
            var start = tuNgay.Date;
            var end = denNgay.Date.AddDays(1);

            // 1. Lấy hóa đơn theo đúng tên cột SQL
            var hoaDons = await _context.HoaDons
                .Where(h => h.ThoiGianVao >= start && h.ThoiGianVao < end)
                .ToListAsync();

            var tongDoanhThu = hoaDons.Sum(h => h.TongTienThanhToan);
            var tongDonHang = hoaDons.Count;
            var khachHangUnique = hoaDons.Select(h => h.MaNguoiDung).Distinct().Count();
            var donTrungBinh = tongDonHang > 0 ? tongDoanhThu / tongDonHang : 0;

            // 2. Thống kê Top 10 món ăn
            var topSanPham = await _context.ChiTietHoaDons
                .Include(ct => ct.MonAn) // Đã hết lỗi nhờ Bước 1
                .Where(ct => ct.HoaDon.ThoiGianVao >= start && ct.HoaDon.ThoiGianVao < end)
                .GroupBy(ct => ct.MonAn.TenMonAn)
                .Select(g => new {
                    TenSanPham = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                TongDoanhThu = tongDoanhThu,
                TongDonHang = tongDonHang,
                KhachHang = khachHangUnique,
                DonTrungBinh = donTrungBinh,
                TopSanPham = topSanPham
            });
        }
    }
}
