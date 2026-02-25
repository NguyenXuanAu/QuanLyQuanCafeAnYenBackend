using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using QuanLyQuanCafeAnYenBackend.Models;
using QuanLyQuanCafeAnYenBackend.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization; 

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthStaffController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly JWTSetting _appSettings;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public AuthStaffController(
            QuanLyQuanCafeDbContext context,
            IOptions<JWTSetting> appSettings,
            IConfiguration config,
            IMemoryCache cache)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _config = config;
            _cache = cache;
        }

        //Frontend gọi về nhằm kích hoạt Middleware kiểm tra Stamp
        [Authorize]
        [HttpGet("CheckToken")]
        public IActionResult CheckToken()
        {
            return Ok(new { Success = true, Message = "Token hợp lệ." });
        }

        private string SimpleHash(string password)
        {
            string secretSalt = "AnYenCoffee_Secret_2024";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password + secretSalt));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        private string GenerateJwtToken(NhanVien staff)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("StaffId", staff.MaNhanVien.Trim()),
                    new Claim("FullName", staff.HoTen),
                    new Claim(ClaimTypes.Role, staff.ChucVu),
                    new Claim("Type", "Staff"),
                    new Claim("SecurityStamp", staff.SecurityStamp ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtHandler.CreateToken(tokenDescriptor);
            return jwtHandler.WriteToken(token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            string hashedInput = SimpleHash(model.Password);

            var staff = await _context.NhanViens
                .FirstOrDefaultAsync(s =>
                    (s.SoDienThoai.Trim() == model.Phone.Trim() || s.Email.Trim() == model.Phone.Trim() || s.MaNhanVien.Trim() == model.Phone.Trim())
                    && s.MatKhauHash == hashedInput);

            if (staff == null)
            {
                return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
            }

            if (staff.TrangThai == false)
            {
                return BadRequest(new { Message = "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động" });
            }

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập hệ thống quản lý thành công!",
                Token = GenerateJwtToken(staff),
                StaffInfo = new { staff.MaNhanVien, staff.HoTen, staff.ChucVu }
            });
        }

        [HttpPost("LogoutAllDevices")]
        public async Task<IActionResult> LogoutAllDevices([FromBody] string staffId)
        {
            string cleanId = staffId.Replace("\"", "").Trim();
            var staff = await _context.NhanViens.FirstOrDefaultAsync(s => s.MaNhanVien.Trim() == cleanId);

            if (staff == null) return NotFound(new { Message = "Không tìm thấy nhân viên" });
            staff.SecurityStamp = Guid.NewGuid().ToString();

            _context.NhanViens.Update(staff);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đã vô hiệu hóa toàn bộ Token cũ!" });
        }

        [HttpPost("ForgotStaffPassword")]
        public async Task<IActionResult> ForgotStaffPassword([FromBody] string account)
        {
            var staff = await _context.NhanViens.FirstOrDefaultAsync(s =>
                s.SoDienThoai == account || s.Email == account || s.MaNhanVien == account);

            if (staff == null || string.IsNullOrEmpty(staff.Email))
            {
                return NotFound(new { Message = "Không tìm thấy thông tin nhân viên hoặc Email liên kết" });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"StaffOTP_{staff.Email}", otp, TimeSpan.FromMinutes(5));

            try
            {
                await SendEmailAsync(staff.Email, "Mã OTP phục hồi tài khoản NHÂN VIÊN - An Yên Coffee",
                    $"<h3>Xin chào {staff.HoTen},</h3><p>Mã xác thực để đặt lại mật khẩu của bạn là: <b style='color:red; font-size:20px;'>{otp}</b></p>");
                return Ok(new { Success = true, Message = "Mã OTP đã gửi về Email nhân viên", Email = staff.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi gửi Email: " + ex.Message });
            }
        }

        [HttpPost("ResetStaffPassword")]
        public async Task<IActionResult> ResetStaffPassword([FromBody] ResetPasswordDto model)
        {
            if (!_cache.TryGetValue($"StaffOTP_{model.Email}", out string savedOtp) || savedOtp != model.Otp)
            {
                return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn" });
            }

            var staff = await _context.NhanViens.FirstOrDefaultAsync(s => s.Email == model.Email);
            if (staff == null) return NotFound();

            staff.MatKhauHash = SimpleHash(model.NewPassword);
            staff.SecurityStamp = Guid.NewGuid().ToString();

            _context.NhanViens.Update(staff);
            await _context.SaveChangesAsync();

            _cache.Remove($"StaffOTP_{model.Email}");
            return Ok(new { Success = true, Message = "Đặt lại mật khẩu thành công!" });
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["MailSettings:Mail"];
            var password = _config["MailSettings:Password"];
            var host = _config["MailSettings:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["MailSettings:Port"] ?? "587");

            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, "An Yên Coffee Management"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}