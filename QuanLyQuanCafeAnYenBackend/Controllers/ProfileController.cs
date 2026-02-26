using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public ProfileController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpGet("{email}")]
        public IActionResult GetProfile(string email)
        {
            var user = _context.NguoiDungs   // 👈 đổi đúng DbSet của bạn
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.MaNguoiDung,
                    u.HoTen,
                    u.Email,
                    u.SoDienThoai,
                    u.DoTuoi,
                    u.NgayTao
                })
                .FirstOrDefault();

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        [HttpPut("update/{email}")]
        public IActionResult UpdateProfile(string email, [FromBody] UpdateProfileDto model)
        {
            var user = _context.NguoiDungs.FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            // =====================
            // CẬP NHẬT THÔNG TIN
            // =====================
            user.HoTen = model.HoTen;
            user.SoDienThoai = model.SoDienThoai;
            user.DoTuoi = model.DoTuoi;

            // =====================
            // ĐỔI MẬT KHẨU (KHÔNG CẦN MẬT KHẨU CŨ)
            // =====================
            if (!string.IsNullOrEmpty(model.MatKhauMoi))
            {
                // Phải nhập lại mật khẩu
                if (string.IsNullOrEmpty(model.MatKhauCu))
                {
                    return BadRequest(new { message = "Vui lòng nhập lại mật khẩu" });
                }

                // Hai mật khẩu phải trùng nhau
                if (model.MatKhauMoi != model.MatKhauCu)
                {
                    return BadRequest(new { message = "Mật khẩu nhập lại không khớp" });
                }

                // Không được trùng mật khẩu hiện tại
                if (user.MatKhauHash == model.MatKhauMoi)
                {
                    return BadRequest(new { message = "Mật khẩu mới không được trùng mật khẩu cũ" });
                }

                // Cập nhật mật khẩu mới
                user.MatKhauHash = model.MatKhauMoi;
            }

            _context.SaveChanges();

            return Ok(new { message = "Cập nhật thành công" });
        }

        public class UpdateProfileDto
        {
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
            public int DoTuoi { get; set; }
            public string? MatKhauCu { get; set; }
            public string? MatKhauMoi { get; set; }
        }

    }
}