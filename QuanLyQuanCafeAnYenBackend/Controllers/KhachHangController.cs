using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    public class KhachHangController : Controller
    {
        public readonly QuanLyQuanCafeDbContext _context;
        public KhachHangController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }
        //Lấy full khách hàng
        [HttpGet("Khach-Hang")]
        public IActionResult GetAllKhachHang()
        {
            var khachHangs = _context.NguoiDungs.ToList();
            return Ok(new
            {
                message = "Lấy danh sách khách hàng thành công",
                data = khachHangs
            });
        }
        //Láy 1 khách hàng
        [HttpGet("Khach-Hang/{id}")]
        public IActionResult GetKhachHangById(string id)
        {
            var khachHang = _context.NguoiDungs.FirstOrDefault(kh => kh.MaNguoiDung == id);
            if (khachHang == null)
            {
                return NotFound(new
                {
                    message = "Khách hàng không tồn tại"
                });
            }
            return Ok(new
            {
                message = "Lấy thông tin khách hàng thành công",
                data = khachHang
            });
        }
        //Cập nhật khách hàng
        [HttpPut("Khach-Hang/{id}")]
        public IActionResult UpdateKhachHang(string id, [FromBody] NguoiDung updatedKhachHang)
        {
            var khachHang = _context.NguoiDungs.FirstOrDefault(kh => kh.MaNguoiDung == id);
            if (khachHang == null)
            {
                return NotFound(new
                {
                    message = "Khách hàng không tồn tại"
                });
            }
            khachHang.MatKhauHash = updatedKhachHang.MatKhauHash;
            khachHang.ChuThich = updatedKhachHang.ChuThich;
            _context.SaveChanges();
            return Ok(new
            {
                message = "Cập nhật thông tin khách hàng thành công",
                data = new { khachHang.MaNguoiDung, khachHang.HoTen, khachHang.Email, khachHang.SoDienThoai, khachHang.MatKhauHash, khachHang.ChuThich }
            });
        }
        // Tìm khách hàng
        [HttpGet("Tim-Khach-Hang")]
        public IActionResult SearchKhachHang([FromQuery] string keyword)
        {
            var khachHangs = _context.NguoiDungs
                .Where(kh => kh.HoTen.Contains(keyword) || kh.Email.Contains(keyword) || kh.SoDienThoai.Contains(keyword) || kh.MaNguoiDung.Contains(keyword) || kh.DiemTichLuy.ToString().Contains(keyword) || kh.VaiTro.ToString().Contains(keyword))
                .ToList();
            return Ok(new
            {
                message = "Tìm kiếm khách hàng thành công",
                data = khachHangs
            });
        }
    }
}
