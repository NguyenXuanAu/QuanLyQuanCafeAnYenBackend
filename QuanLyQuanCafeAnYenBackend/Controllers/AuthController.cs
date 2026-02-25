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
    public class AuthController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly JWTSetting _appSettings;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;

        public AuthController(QuanLyQuanCafeDbContext context, IOptions<JWTSetting> appSettings, IConfiguration config, IMemoryCache cache)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _config = config;
            _cache = cache;
        }

        // THÊM: Kiểm tra Token cho khách hàng tương tự Admin
        [Authorize]
        [HttpGet("CheckToken")]
        public IActionResult CheckToken()
        {
            return Ok(new { Success = true, Message = "Token khách hàng hợp lệ." });
        }

        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.MaNguoiDung.Trim()),
                    new Claim("FullName", user.HoTen),
                    new Claim(ClaimTypes.Role, user.VaiTro == 1 ? "Admin" : "User"),
                    new Claim("Type", "Customer"), // Để phân biệt với "Staff"
                    new Claim("SecurityStamp", user.SecurityStamp ?? "") // Thêm Stamp vào Token
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtHandler.CreateToken(tokenDescriptor);
            return jwtHandler.WriteToken(token);
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            string hashedInput = SimpleHash(model.Password);
            string phone = model.Phone.Trim();
            // 1. Kiểm tra Người dùng
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => (u.SoDienThoai.Trim() == phone || u.Email.Trim() == phone)
                                     && u.MatKhauHash == hashedInput);

            if (user != null)
            {
                // Tự động sinh mã bảo mật nếu chưa có
                if (string.IsNullOrEmpty(user.SecurityStamp))
                {
                    user.SecurityStamp = Guid.NewGuid().ToString();
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    Success = true,
                    IsAdminAccount = false,
                    Token = GenerateJwtToken(user),
                    User = new { user.HoTen, user.Email }
                });
            }
            // 2. Kiểm tra nếu là Nhân viên thì báo chuyển trang
            var staff = await _context.NhanViens
                .FirstOrDefaultAsync(s => (s.SoDienThoai.Trim() == phone || s.Email.Trim() == phone || s.MaNhanVien.Trim() == phone)
                                     && s.MatKhauHash == hashedInput);

            if (staff != null)
            {
                return Ok(new { Success = true, IsAdminAccount = true, Message = "Tài khoản nhân viên, vui lòng qua cổng Admin" });
            }

            return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
        }
        // THÊM: Đăng xuất tất cả thiết bị cho khách hàng
        [HttpPost("LogoutAllDevices")]
        public async Task<IActionResult> LogoutAllDevices([FromBody] string userId)
        {
            string cleanId = userId.Replace("\"", "").Trim();
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung.Trim() == cleanId);

            if (user == null) return NotFound(new { Message = "Không tìm thấy người dùng" });

            user.SecurityStamp = Guid.NewGuid().ToString();
            _context.NguoiDungs.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đã vô hiệu hóa toàn bộ Token khách hàng cũ!" });
        }

        [HttpPost("Forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account)
        {
            // 1. Làm sạch dữ liệu đầu vào
            string cleanAccount = account?.Replace("\"", "").Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(cleanAccount))
                return BadRequest(new { Message = "Thông tin tài khoản không được để trống" });

            // 2. Tìm kiếm ở cả 2 bảng: Người dùng và Nhân viên
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == cleanAccount || u.Email == cleanAccount);
            var staff = (user == null) ? await _context.NhanViens.FirstOrDefaultAsync(s => s.SoDienThoai == cleanAccount || s.Email == cleanAccount || s.MaNhanVien == cleanAccount) : null;

            // 3. Xác định Email và Tên người nhận
            var targetEmail = (user?.Email ?? staff?.Email)?.ToLower().Trim();
            var targetName = user?.HoTen ?? staff?.HoTen;

            if (string.IsNullOrEmpty(targetEmail))
            {
                return NotFound(new { Message = "Không tìm thấy tài khoản liên kết với thông tin này" });
            }
            // 4. Sinh mã OTP dùng chung một Prefix Global
            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set($"GlobalOTP_{targetEmail}", otp, TimeSpan.FromMinutes(5));

            try
            {
                await SendEmailAsync(targetEmail, "Mã OTP phục hồi mật khẩu - An Yên Coffee",
                    $"<h3>Xin chào {targetName},</h3><p>Mã OTP của bạn là: <b style='color:red; font-size:20px;'>{otp}</b></p>");

                return Ok(new { Success = true, Message = "Mã OTP đã được gửi về Email của bạn", Email = targetEmail });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi gửi Email: " + ex.Message });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            string cleanEmail = model.Email?.Replace("\"", "").Trim().ToLower() ?? "";

            // Kiểm tra OTP từ bộ nhớ đệm dùng chung
            if (!_cache.TryGetValue($"GlobalOTP_{cleanEmail}", out string savedOtp) || savedOtp != model.Otp?.Trim())
            {
                return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn" });
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == cleanEmail);
            var staff = await _context.NhanViens.FirstOrDefaultAsync(s => s.Email == cleanEmail);

            if (user == null && staff == null) return NotFound();

            string hashedPass = SimpleHash(model.NewPassword);
            string newStamp = Guid.NewGuid().ToString(); // Tạo Stamp mới để vô hiệu hóa Token cũ

            if (user != null)
            {
                user.MatKhauHash = hashedPass;
                user.SecurityStamp = newStamp;
            }
            if (staff != null)
            {
                staff.MatKhauHash = hashedPass;
                staff.SecurityStamp = newStamp;
            }

            await _context.SaveChangesAsync();
            _cache.Remove($"GlobalOTP_{cleanEmail}");

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
                From = new MailAddress(fromEmail, "An Yên Coffee"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);
            await smtpClient.SendMailAsync(mailMessage);
        }

    }
}