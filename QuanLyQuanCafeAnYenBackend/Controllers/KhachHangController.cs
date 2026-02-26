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

        // --- HÀM HỖ TRỢ KIỂM TRA VÀ TỰ ĐỘNG MỞ KHÓA ---
        private bool KiemTraVaMoKhoa(NguoiDung user)
        {
            if (string.IsNullOrEmpty(user.ChuThich)) return false;

            if (user.ChuThich.Contains("lần 3")) return false;

            int timeIndex = user.ChuThich.IndexOf("- Thời gian:");
            if (timeIndex == -1) return false;

            string timeString = user.ChuThich.Substring(timeIndex + 12).Trim();
            if (DateTime.TryParse(timeString, out DateTime thoiGianKhoa))
            {
                bool daHetHan = false;
                if (user.ChuThich.Contains("lần 1") && DateTime.Now >= thoiGianKhoa.AddDays(1))
                {
                    daHetHan = true;
                }
                else if (user.ChuThich.Contains("lần 2") && DateTime.Now >= thoiGianKhoa.AddDays(7))
                {
                    daHetHan = true;
                }

                if (daHetHan)
                {
                    user.ChuThich = null;
                    return true; // Trả về true nếu có thay đổi
                }
            }
            return false;
        }

        [HttpGet("Khach-Hang")]
        public IActionResult GetAllKhachHang()
        {
            var khachHangs = _context.NguoiDungs.ToList();

            // Quét và mở khóa tự động
            bool hasChanges = false;
            foreach (var kh in khachHangs)
            {
                if (KiemTraVaMoKhoa(kh)) hasChanges = true;
            }
            if (hasChanges) _context.SaveChanges();

            return Ok(new
            {
                message = "Lấy danh sách khách hàng thành công",
                data = khachHangs
            });
        }

        [HttpGet("Khach-Hang/{id}")]
        public IActionResult GetKhachHangById(string id)
        {
            var khachHang = _context.NguoiDungs.FirstOrDefault(kh => kh.MaNguoiDung == id);
            if (khachHang == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại" });
            }

            // Kiểm tra và mở khóa
            if (KiemTraVaMoKhoa(khachHang))
            {
                _context.SaveChanges();
            }

            return Ok(new
            {
                message = "Lấy thông tin khách hàng thành công",
                data = khachHang
            });
        }

        [HttpPut("Khach-Hang/{id}")]
        public IActionResult UpdateKhachHang(string id, [FromBody] NguoiDung updatedKhachHang)
        {
            var khachHang = _context.NguoiDungs.FirstOrDefault(kh => kh.MaNguoiDung == id);
            if (khachHang == null)
            {
                return NotFound(new { message = "Khách hàng không tồn tại" });
            }
            khachHang.MatKhauHash = updatedKhachHang.MatKhauHash;
            khachHang.ChuThich = updatedKhachHang.ChuThich;

            // Nếu admin khóa thủ công từ giao diện, gắn thêm thời gian để auto-unlock chạy được
            if (!string.IsNullOrEmpty(khachHang.ChuThich) && !khachHang.ChuThich.Contains("- Thời gian:"))
            {
                if (khachHang.ChuThich.Contains("lần 1") || khachHang.ChuThich.Contains("lần 2"))
                {
                    khachHang.ChuThich += $" - Thời gian: {DateTime.Now:O}";
                }
            }

            _context.SaveChanges();
            return Ok(new
            {
                message = "Cập nhật thông tin khách hàng thành công",
                data = new { khachHang.MaNguoiDung, khachHang.HoTen, khachHang.Email, khachHang.SoDienThoai, khachHang.MatKhauHash, khachHang.ChuThich }
            });
        }

        [HttpGet("Tim-Khach-Hang")]
        public IActionResult SearchKhachHang([FromQuery] string keyword)
        {
            var khachHangs = _context.NguoiDungs
                .Where(kh => kh.HoTen.Contains(keyword) || kh.Email.Contains(keyword) || kh.SoDienThoai.Contains(keyword) || kh.MaNguoiDung.Contains(keyword) || kh.DiemTichLuy.ToString().Contains(keyword) || kh.VaiTro.ToString().Contains(keyword))
                .ToList();

            // Quét và mở khóa
            bool hasChanges = false;
            foreach (var kh in khachHangs)
            {
                if (KiemTraVaMoKhoa(kh)) hasChanges = true;
            }
            if (hasChanges) _context.SaveChanges();

            return Ok(new
            {
                message = "Tìm kiếm khách hàng thành công",
                data = khachHangs
            });
        }
    }
}