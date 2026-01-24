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

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    // LỚP NHẬN DỮ LIỆU ĐĂNG KÝ (DTO) - Giúp sửa lỗi 400 Bad Request
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

        // --- 1. ĐĂNG KÝ (Đã sửa lỗi 400 và tự động tăng mã ND0XX) ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest data)
        {
            if (data == null) return BadRequest(new { Message = "Dữ liệu gửi lên không hợp lệ!" });

            // Kiểm tra trùng lặp
            if (await _context.NguoiDungs.AnyAsync(u => u.SoDienThoai == data.Phone || u.Email == data.Email))
            {
                return BadRequest(new { Success = false, Message = "Số điện thoại hoặc Email đã tồn tại!" });
            }

            // LOGIC TỰ ĐỘNG TẠO MÃ ND0XX
            var lastUser = await _context.NguoiDungs
                .Where(u => u.MaNguoiDung.StartsWith("ND"))
                .OrderByDescending(u => u.MaNguoiDung)
                .FirstOrDefaultAsync();

            string newMaND;
            if (lastUser == null)
            {
                newMaND = "ND001";
            }
            else
            {
                // Lấy phần số sau chữ "ND", cộng 1 và định dạng 3 chữ số (D3)
                int lastNumber = int.Parse(lastUser.MaNguoiDung.Substring(2));
                newMaND = "ND" + (lastNumber + 1).ToString("D3");
            }

            // Tạo đối tượng NguoiDung mới khớp với bảng Database
            var newUser = new NguoiDung
            {
                MaNguoiDung = newMaND,
                HoTen = data.Name,
                SoDienThoai = data.Phone,
                Email = data.Email,
                MatKhauHash = SimpleHash(data.Password),
                VaiTro = 0, // Mặc định là Người dùng
                DiemTichLuy = 0,
                NgayTao = DateTime.Now,
                ChuThich = data.Age // Lưu loại độ tuổi vào chú thích
            };

            // Gán giá trị số cho trường DoTuoi (int)
            if (data.Age == "child") newUser.DoTuoi = 15;
            else if (data.Age == "adult") newUser.DoTuoi = 25;
            else if (data.Age == "senior") newUser.DoTuoi = 65;

            _context.NguoiDungs.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, Message = "Đăng ký thành công!", MaNguoiDung = newMaND });
        }

        // --- 2. ĐĂNG NHẬP ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            string hashedInput = SimpleHash(model.Password);

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u =>
                    (u.SoDienThoai.Trim() == model.Phone.Trim() || u.Email.Trim() == model.Phone.Trim())
                    && u.MatKhauHash == hashedInput);

            if (user == null)
            {
                return Unauthorized(new { Success = false, Message = "Tài khoản hoặc mật khẩu không chính xác" });
            }

            return Ok(new
            {
                Success = true,
                Message = "Đăng nhập thành công!",
                User = new { user.HoTen, user.SoDienThoai, user.Email }
            });
        }

        // --- 3. QUÊN MẬT KHẨU (Gửi OTP) ---
        [HttpPost("Forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string account)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == account || u.Email == account);
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                return NotFound(new { Message = "Không tìm thấy tài khoản liên kết với Email" });
            }

            string otp = new Random().Next(100000, 999999).ToString();
            _cache.Set(user.Email, otp, TimeSpan.FromMinutes(5));

            try
            {
                await SendEmailAsync(user.Email, "Mã OTP đặt lại mật khẩu - An Yên Coffee",
                    $"<h3>Mã OTP của bạn là: <b style='color:red;'>{otp}</b></h3><p>Mã này có hiệu lực trong 5 phút.</p>");

                return Ok(new { Success = true, Message = "Mã OTP đã được gửi về Email của bạn", Email = user.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Lỗi gửi Email: " + ex.Message });
            }
        }

        // --- 4. ĐẶT LẠI MẬT KHẨU ---
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!_cache.TryGetValue(model.Email, out string savedOtp) || savedOtp != model.Otp)
            {
                return BadRequest(new { Message = "Mã OTP không chính xác hoặc đã hết hạn" });
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return NotFound();

            user.MatKhauHash = SimpleHash(model.NewPassword);
            _context.NguoiDungs.Update(user);
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