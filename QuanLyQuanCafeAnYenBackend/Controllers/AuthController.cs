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
                    new Claim("FullName", user.HoTen ?? ""),
                    new Claim(ClaimTypes.Role, user.VaiTro == 1 ? "Admin" : "User"),
                    new Claim("Type", "Customer"),
                    new Claim("SecurityStamp", user.SecurityStamp ?? Guid.NewGuid().ToString())
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
            try
            {
                // Validate input
                if (model == null || string.IsNullOrEmpty(model.Phone) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { Success = false, Message = "Vui lòng nhập đầy đủ thông tin!" });
                }

                string hashedInput = SimpleHash(model.Password);
                string phone = model.Phone?.Trim() ?? "";

                // ===== SỬA LOGIC KIỂM TRA =====
                // Bước 1: Tìm user theo SĐT hoặc Email (KHÔNG kiểm tra mật khẩu)
                var user = await _context.NguoiDungs
    .FirstOrDefaultAsync(u =>
        (u.SoDienThoai != null && u.SoDienThoai.Trim() == phone) ||
        (u.Email != null && u.Email.Trim() == phone));

                var staff = await _context.NhanViens
                    .FirstOrDefaultAsync(s =>
                        (s.SoDienThoai != null && s.SoDienThoai.Trim() == phone) ||
                        (s.Email != null && s.Email.Trim() == phone) ||
                        (s.MaNhanVien != null && s.MaNhanVien.Trim() == phone));

                // Bước 2: Nếu tìm thấy user, kiểm tra mật khẩu
                if (user != null)
                {
                    if (user.MatKhauHash != hashedInput)
                    {
                        // Sai mật khẩu - trả về lỗi 401
                        return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
                    }

                    // Đúng mật khẩu - xử lý SecurityStamp
                    try
                    {
                        if (string.IsNullOrEmpty(user.SecurityStamp))
                        {
                            user.SecurityStamp = Guid.NewGuid().ToString();
                            _context.NguoiDungs.Update(user);
                            await _context.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log lỗi nhưng vẫn cho đăng nhập
                        Console.WriteLine($"Lỗi cập nhật SecurityStamp: {ex.Message}");
                    }

                    return Ok(new
                    {
                        Success = true,
                        IsAdminAccount = false,
                        Token = GenerateJwtToken(user),
                        User = new
                        {
                            user.HoTen,
                            user.Email,
                            user.SoDienThoai,
                            user.MaNguoiDung
                        }
                    });
                }

                // Bước 3: Nếu không phải user, kiểm tra staff
                

                if (staff != null)
                {
                    // Kiểm tra mật khẩu staff
                    if (staff.MatKhauHash != hashedInput)
                    {
                        return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
                    }

                    // Staff đăng nhập nhầm cổng
                    return Ok(new
                    {
                        Success = true,
                        IsAdminAccount = true,
                        Message = "Tài khoản nhân viên, vui lòng qua cổng Admin"
                    });
                }

                // Bước 4: Không tìm thấy tài khoản
                return Unauthorized(new { Success = false, Message = "Tài khoản không tồn tại!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("LogoutAllDevices")]
        public async Task<IActionResult> LogoutAllDevices([FromBody] string userId)
        {
            try
            {
                string cleanId = userId?.Replace("\"", "").Trim() ?? "";
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung.Trim() == cleanId);

                if (user == null)
                    return NotFound(new { Message = "Không tìm thấy người dùng" });

                // Đổi Stamp để vô hiệu hóa toàn bộ Token cũ
                user.SecurityStamp = Guid.NewGuid().ToString();
                await _context.SaveChangesAsync();

                return Ok(new { Success = true, Message = "Đã vô hiệu hóa toàn bộ phiên đăng nhập cũ!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost("Forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account)
        {
            try
            {
                string cleanAccount = account?.Replace("\"", "").Trim().ToLower() ?? "";
                if (string.IsNullOrEmpty(cleanAccount))
                    return BadRequest(new { Message = "Thông tin tài khoản không được để trống" });

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == cleanAccount || u.Email == cleanAccount);
                var staff = (user == null) ? await _context.NhanViens.FirstOrDefaultAsync(s => s.SoDienThoai == cleanAccount || s.Email == cleanAccount || s.MaNhanVien == cleanAccount) : null;

                var targetEmail = (user?.Email ?? staff?.Email)?.ToLower().Trim();
                var targetName = user?.HoTen ?? staff?.HoTen;

                if (string.IsNullOrEmpty(targetEmail))
                    return NotFound(new { Message = "Không tìm thấy tài khoản liên kết" });

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
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            try
            {
                string cleanEmail = model.Email?.Replace("\"", "").Trim().ToLower() ?? "";

                if (!_cache.TryGetValue($"GlobalOTP_{cleanEmail}", out string savedOtp) || savedOtp != model.Otp?.Trim())
                {
                    return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn" });
                }

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == cleanEmail);
                var staff = await _context.NhanViens.FirstOrDefaultAsync(s => s.Email == cleanEmail);

                if (user == null && staff == null)
                    return NotFound(new { Message = "Không tìm thấy tài khoản" });

                string hashedPass = SimpleHash(model.NewPassword);
                string newStamp = Guid.NewGuid().ToString();

                if (user != null)
                {
                    user.MatKhauHash = hashedPass;
                    user.SecurityStamp = newStamp;
                    _context.NguoiDungs.Update(user);
                }
                if (staff != null)
                {
                    staff.MatKhauHash = hashedPass;
                    staff.SecurityStamp = newStamp;
                    _context.NhanViens.Update(staff);
                }

                await _context.SaveChangesAsync();
                _cache.Remove($"GlobalOTP_{cleanEmail}");

                return Ok(new { Success = true, Message = "Đặt lại mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = _config["MailSettings:Mail"];
            var password = _config["MailSettings:Password"];
            var host = _config["MailSettings:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["MailSettings:Port"] ?? "587");

            using var smtpClient = new SmtpClient(host)
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

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            // Kiểm tra số điện thoại đã tồn tại chưa
            if (await _context.NguoiDungs.AnyAsync(u => u.SoDienThoai == model.Phone))
                return BadRequest(new { success = false, message = "Số điện thoại này đã được đăng ký!" });

            // Tạo mã người dùng mới (VD: ND011)
            var lastUser = await _context.NguoiDungs.OrderByDescending(u => u.MaNguoiDung).FirstOrDefaultAsync();
            int nextId = 1;
            if (lastUser != null && lastUser.MaNguoiDung.StartsWith("ND"))
            {
                int.TryParse(lastUser.MaNguoiDung.Substring(2), out nextId);
                nextId++;
            }

            var newUser = new NguoiDung
            {
                MaNguoiDung = $"ND{nextId:D3}",
                HoTen = model.Name,
                SoDienThoai = model.Phone,
                Email = model.Email,
                MatKhauHash = SimpleHash(model.Password), // Dùng hàm băm có sẵn trong AuthController
                DoTuoi = int.Parse(model.Age),
                NgayTao = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đăng ký thành công!" });
        }

        // Thêm class này vào cuối file AuthController.cs (ngoài class chính) để hứng dữ liệu
        public class RegisterRequest
        {
            public string Name { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Age { get; set; }
        }
    }
}