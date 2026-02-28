using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
using System.IO;
using System;
using System.Linq;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiaController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DanhGiaController(QuanLyQuanCafeDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- LẤY DANH SÁCH ĐÁNH GIÁ ---
        [HttpGet("all")]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _context.DanhGias
                .OrderByDescending(d => d.NgayDanhGia)
                .Select(d => new
                {
                    maDanhGia = d.MaDanhGia,
                    soSao = d.SoSao,
                    binhLuan = d.BinhLuan,
                    ngayDanhGia = d.NgayDanhGia,
                    maHoaDon = d.MaHoaDon,
                    maNguoiDung = d.MaNguoiDung,
                    tenNguoiDung = _context.NguoiDungs
                                    .Where(n => n.MaNguoiDung == d.MaNguoiDung)
                                    .Select(n => n.HoTen)
                                    .FirstOrDefault() ?? "Khách hàng ẩn danh",
                    hinhAnhs = _context.HinhAnhDanhGias
                                    .Where(h => h.MaDanhGia == d.MaDanhGia)
                                    .OrderBy(h => h.ThuTuHienThi)
                                    .Select(h => h.DuongDanAnh)
                                    .ToList()
                })
                .ToListAsync();

            return Ok(feedbacks);
        }
        // DELETE: api/DanhGia/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var danhGia = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaDanhGia == id);

            if (danhGia == null)
                return NotFound("Không tìm thấy đánh giá.");

            _context.DanhGias.Remove(danhGia);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Xóa đánh giá thành công!" });
        }
        // --- GỬI ĐÁNH GIÁ MỚI ---
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback([FromForm] DanhGiaInputDto input)
        {
            // 1. KIỂM TRA MỖI NGƯỜI DÙNG CHỈ ĐƯỢC ĐÁNH GIÁ 1 LẦN
            var alreadyReviewed = await _context.DanhGias.AnyAsync(d => d.MaNguoiDung == input.MaNguoiDung);
            if (alreadyReviewed)
            {
                return BadRequest("Bạn đã gửi đánh giá rồi. Mỗi tài khoản chỉ được đánh giá 1 lần duy nhất!");
            }

            // 2. BỘ LỌC TỪ NGỮ & KHÓA TÀI KHOẢN
            string[] badWords = { "dm", "vcl", "fuck", "shit", "ngu", "lon" };

            if (!string.IsNullOrEmpty(input.BinhLuan))
            {
                string lowerComment = input.BinhLuan.ToLower();
                bool containsBadWord = badWords.Any(word => lowerComment.Contains(word));

                if (containsBadWord)
                {
                    var user = await _context.NguoiDungs.FindAsync(input.MaNguoiDung);
                    if (user != null)
                    {
                        int currentLevel = 0;
                        if (!string.IsNullOrEmpty(user.ChuThich))
                        {
                            if (user.ChuThich.Contains("lần 3")) currentLevel = 3;
                            else if (user.ChuThich.Contains("lần 2")) currentLevel = 2;
                            else if (user.ChuThich.Contains("lần 1")) currentLevel = 1;
                        }

                        string newViolation = "";
                        string reason = "Sử dụng từ ngữ vi phạm quy chuẩn văn hóa khi đánh giá";

                        if (currentLevel == 0)
                            newViolation = $"Vi phạm chính sách lần 1 (Khóa tài khoản 1 ngày) - Lý do: {reason} - Thời gian: {DateTime.Now:O}";
                        else if (currentLevel == 1)
                            newViolation = $"Vi phạm chính sách lần 2 (Khóa tài khoản 1 tuần) - Lý do: {reason} - Thời gian: {DateTime.Now:O}";
                        else
                            newViolation = $"Vi phạm chính sách lần 3 (Tài khoản đã bị khóa) - Lý do: {reason}";

                        user.ChuThich = newViolation;
                        user.SecurityStamp = Guid.NewGuid().ToString(); // Vô hiệu hóa token hiện tại
                        await _context.SaveChangesAsync();
                    }
                    return StatusCode(403, "Tài khoản của bạn đã bị khóa do sử dụng từ ngữ vi phạm quy chuẩn văn hóa!");
                }
            }

            // 3. XỬ LÝ LƯU ẢNH (Đổi tên theo format: TenGoc_NgayGio.ext)
            string? dbImagePath = null;
            if (input.HinhAnh != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extension = Path.GetExtension(input.HinhAnh.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest("Chỉ chấp nhận file định dạng ảnh (.jpg, .png, .webp, .gif).");
                }

                if (input.HinhAnh.Length > 5 * 1024 * 1024) // Giới hạn 5MB
                {
                    return BadRequest("Kích thước ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.");
                }

                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "feedback");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // Format tên ảnh mới
                string originalName = Path.GetFileNameWithoutExtension(input.HinhAnh.FileName);
                // Xóa ký tự lạ trong tên file cũ
                originalName = string.Join("_", originalName.Split(Path.GetInvalidFileNameChars()));
                string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var uniqueFileName = $"{originalName}_{timeStamp}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await input.HinhAnh.CopyToAsync(fileStream);
                }

                dbImagePath = "/images/feedback/" + uniqueFileName;
            }

            // 4. LƯU ĐÁNH GIÁ VÀO DATABASE
            var lastDG = await _context.DanhGias.OrderByDescending(d => d.MaDanhGia).FirstOrDefaultAsync();
            int nextId = 1;
            if (lastDG != null && lastDG.MaDanhGia.StartsWith("DG"))
            {
                int.TryParse(lastDG.MaDanhGia.Substring(2), out nextId);
                nextId++;
            }
            string maDGNew = $"DG{nextId:D3}";

            var newDanhGia = new DanhGia
            {
                MaDanhGia = maDGNew,
                SoSao = input.SoSao,
                BinhLuan = input.BinhLuan,
                NgayDanhGia = DateTime.Now,
                MaNguoiDung = input.MaNguoiDung,
                MaHoaDon = input.MaHoaDon // Lấy từ form (hiện tại là mã cứng HD001)
            };

            _context.DanhGias.Add(newDanhGia);

            if (dbImagePath != null)
            {
                var newHinhAnh = new HinhAnhDanhGia
                {
                    MaHinhAnh = Guid.NewGuid().ToString("N").Substring(0, 20),
                    DuongDanAnh = dbImagePath,
                    ThuTuHienThi = 1,
                    MaDanhGia = maDGNew
                };
                _context.HinhAnhDanhGias.Add(newHinhAnh);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Cảm ơn bạn đã gửi đánh giá thành công!" });
        }
    }

    public class DanhGiaInputDto
    {
        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
        public string MaNguoiDung { get; set; } = null!;
        public string? MaHoaDon { get; set; }
        public IFormFile? HinhAnh { get; set; }
    }
}
