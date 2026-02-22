using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using QuanLyQuanCafeAnYenBackend.Models;
using QuanLyQuanCafeAnYenBackend.DTOs;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    public class RegisterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Age { get; set; } = string.Empty;
    }

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

        private string GenerateJwtToken(NguoiDung user)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.SecretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("UserId", user.MaNguoiDung),
                    new Claim("FullName", user.HoTen),
                    new Claim(ClaimTypes.Role, user.VaiTro == 1 ? "Admin" : "User"),
                    new Claim("Hometown", "Dak Lak"),
                    new Claim("Project", "An Yen Coffee")
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest data)
        {
            if (data == null) return BadRequest(new { Message = "Dữ liệu không hợp lệ!" });

            if (await _context.NguoiDungs.AnyAsync(u => u.SoDienThoai == data.Phone || u.Email == data.Email))
            {
                return BadRequest(new { Success = false, Message = "Số điện thoại hoặc Email đã tồn tại!" });
            }

            var lastUser = await _context.NguoiDungs
                .Where(u => u.MaNguoiDung.StartsWith("ND"))
                .OrderByDescending(u => u.MaNguoiDung)
                .FirstOrDefaultAsync();

            string newMaND = lastUser == null ? "ND001" : "ND" + (int.Parse(lastUser.MaNguoiDung.Substring(2)) + 1).ToString("D3");

            var newUser = new NguoiDung
            {
                MaNguoiDung = newMaND,
                HoTen = data.Name,
                SoDienThoai = data.Phone,
                Email = data.Email,
                MatKhauHash = SimpleHash(data.Password),
                VaiTro = 0,
                DiemTichLuy = 0,
                NgayTao = DateTime.Now,
                ChuThich = data.Age
            };

            if (data.Age == "child") newUser.DoTuoi = 15;
            else if (data.Age == "adult") newUser.DoTuoi = 25;
            else if (data.Age == "senior") newUser.DoTuoi = 65;

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký thành công!", MaNguoiDung = newMaND });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            string hashedInput = SimpleHash(model.Password);
            string phone = model.Phone.Trim();

            // 1. Kiểm tra ở bảng Người Dùng (Khách hàng)
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => (u.SoDienThoai.Trim() == phone || u.Email.Trim() == phone)
                                     && u.MatKhauHash == hashedInput);

            if (user != null)
            {
                // Nếu là Admin/Nhân viên nhưng đăng nhập ở bảng Người dùng
                if (user.VaiTro == 1)
                {
                    return Ok(new { Success = true, IsAdminAccount = true, Username = phone });
                }

                return Ok(new
                {
                    Success = true,
                    Token = GenerateJwtToken(user),
                    User = new { user.HoTen, user.Email }
                });
            }

            // 2. Nếu không thấy ở bảng Người dùng, kiểm tra tiếp bảng Nhân Viên
            var staff = await _context.NhanViens
                .FirstOrDefaultAsync(s => (s.SoDienThoai.Trim() == phone || s.Email.Trim() == phone || s.MaNhanVien.Trim() == phone)
                                     && s.MatKhauHash == hashedInput);

            if (staff != null)
            {
                // Nếu tìm thấy ở bảng nhân viên, yêu cầu chuyển sang cổng Admin
                return Ok(new { Success = true, IsAdminAccount = true, Username = phone });
            }

            return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
        }

        // --- HÀM QUÊN MẬT KHẨU TÍCH HỢP 2 BẢNG ---
        [HttpPost("Forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account)
        {
            // 1. Kiểm tra ở bảng Người dùng
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == account || u.Email == account);

            // 2. Kiểm tra ở bảng Nhân viên nếu không thấy người dùng
            var staff = (user == null) ? await _context.NhanViens.FirstOrDefaultAsync(s => s.SoDienThoai == account || s.Email == account || s.MaNhanVien == account) : null;

            var targetEmail = user?.Email ?? staff?.Email;
            var targetName = user?.HoTen ?? staff?.HoTen;

            if (string.IsNullOrEmpty(targetEmail))
            {
                return NotFound(new { Message = "Không tìm thấy tài khoản liên kết với thông tin này" });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(targetEmail, otp, TimeSpan.FromMinutes(5));

            try
            {
                await SendEmailAsync(targetEmail, "Mã OTP phục hồi mật khẩu - An Yên Coffee",
                    $"<h3>Xin chào {targetName},</h3><p>Mã OTP của bạn là: <b style='color:red;'>{otp}</b></p><p>Mã có hiệu lực trong 5 phút.</p>");
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
            if (!_cache.TryGetValue(model.Email, out string savedOtp) || savedOtp != model.Otp)
            {
                return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn" });
            }

            // Tìm ở cả 2 bảng để cập nhật
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            var staff = await _context.NhanViens.FirstOrDefaultAsync(s => s.Email == model.Email);

            if (user == null && staff == null) return NotFound();

            string hashedPass = SimpleHash(model.NewPassword);

            if (user != null) user.MatKhauHash = hashedPass;
            if (staff != null) staff.MatKhauHash = hashedPass;

            await _context.SaveChangesAsync();
            _cache.Remove(model.Email);
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