using Microsoft.AspNetCore.Mvc;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // ==========================================
        // HELPER PRIVATE METHOD (Xử lý lưu file)
        // ==========================================
        private async Task<(bool IsSuccess, string? Message, object? Data)> SaveFileAsync(IFormFile file, string folder)
        {
            // 1. Validate File
            if (file == null || file.Length == 0)
                return (false, "File không hợp lệ", null);

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return (false, $"Đuôi file {extension} không được hỗ trợ", null);

            if (file.Length > 5 * 1024 * 1024)
                return (false, "Dung lượng file quá lớn (>5MB)", null);

            try
            {
                // 2. Xác định đường dẫn gốc (wwwroot) - FIX PATH
                string webRootPath = _environment.WebRootPath;
                if (string.IsNullOrEmpty(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                }

                // Đảm bảo wwwroot tồn tại
                if (!Directory.Exists(webRootPath))
                {
                    Directory.CreateDirectory(webRootPath);
                }

                // Log để debug
                Console.WriteLine("📁 WebRootPath: " + webRootPath);
                Console.WriteLine("📁 Current Directory: " + Directory.GetCurrentDirectory());

                // 3. Tạo đường dẫn lưu file vật lý - NORMALIZE PATH
                var normalizedFolder = folder.Trim().Trim('/').Replace('/', Path.DirectorySeparatorChar);
                var folderPath = Path.Combine(webRootPath, "images", normalizedFolder);

                Console.WriteLine("📂 FolderPath: " + folderPath);

                // Tạo tất cả thư mục cha nếu chưa tồn tại (QUAN TRỌNG)
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("✅ Đã tạo thư mục: " + folderPath);
                }
                else
                {
                    Console.WriteLine("✅ Thư mục đã tồn tại: " + folderPath);
                }

                // 4. Tạo tên file theo format: YYYYMMDD_HHMMSS_tên-gốc.jpg
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var originalNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);

                // Loại bỏ ký tự đặc biệt, chỉ giữ chữ, số, gạch ngang, gạch dưới
                var cleanName = System.Text.RegularExpressions.Regex.Replace(originalNameWithoutExt, @"[^a-zA-Z0-9_-]", "");

                // Nếu tên quá dài, cắt ngắn còn 30 ký tự
                if (cleanName.Length > 30)
                {
                    cleanName = cleanName.Substring(0, 30);
                }

                var fileName = $"{timestamp}_{cleanName}{extension}";
                var filePath = Path.Combine(folderPath, fileName);

                Console.WriteLine("💾 FilePath: " + filePath);
                Console.WriteLine($"📝 Original: {file.FileName} → New: {fileName}");

                // 5. Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine($"✅ Upload thành công: {fileName}");

                // 6. Trả về URL (Luôn dùng dấu / cho URL web)
                var normalizedFolderUrl = folder.Trim().Trim('/').Replace('\\', '/');
                var fileUrl = $"/images/{normalizedFolderUrl}/{fileName}";

                return (true, "Thành công", new
                {
                    originalName = file.FileName,
                    fileName = fileName,
                    fileUrl = fileUrl,
                    fullUrl = $"{Request.Scheme}://{Request.Host}{fileUrl}"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi upload: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return (false, ex.Message, null);
            }
        }

        // ==========================================
        // API ENDPOINTS
        // ==========================================

        [HttpPost("Single")]
        public async Task<IActionResult> UploadSingle(IFormFile file, [FromQuery] string folder = "tang")
        {
            Console.WriteLine($"🔍 Nhận request upload: folder={folder}, file={file?.FileName}");

            var result = await SaveFileAsync(file, folder);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Upload thành công", data = result.Data });
        }

        [HttpPost("Multiple")]
        public async Task<IActionResult> UploadMultiple(List<IFormFile> files, [FromQuery] string folder = "tang")
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "Không có file được chọn" });

            Console.WriteLine($"🔍 Nhận request upload {files.Count} files vào folder: {folder}");

            var uploadedFiles = new List<object>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                var result = await SaveFileAsync(file, folder);
                if (result.IsSuccess)
                {
                    uploadedFiles.Add(result.Data!);
                }
                else
                {
                    errors.Add($"{file.FileName}: {result.Message}");
                }
            }

            return Ok(new
            {
                message = $"Upload thành công {uploadedFiles.Count}/{files.Count} file",
                data = uploadedFiles,
                errors = errors.Count > 0 ? errors : null
            });
        }

        // Các API con gọi lại logic trên
        [HttpPost("Tang/{maTang}")]
        public async Task<IActionResult> UploadForTang(string maTang, List<IFormFile> files)
            => await UploadMultiple(files, $"tang/{maTang.ToLower()}");

        [HttpPost("MonAn/{maMonAn}")]
        public async Task<IActionResult> UploadForMonAn(string maMonAn, List<IFormFile> files)
            => await UploadMultiple(files, $"monan/{maMonAn.ToLower()}");

        [HttpPost("DanhMuc/{maDanhMuc}")]
        public async Task<IActionResult> UploadForDanhMuc(string maDanhMuc, List<IFormFile> files)
            => await UploadMultiple(files, $"danhmuc/{maDanhMuc.ToLower()}");

        [HttpPost("Ban/{maBan}")]
        public async Task<IActionResult> UploadForBan(string maBan, List<IFormFile> files)
            => await UploadMultiple(files, $"ban/{maBan.ToLower()}");

        [HttpDelete]
        public IActionResult DeleteFile([FromQuery] string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest(new { message = "URL không hợp lệ" });

            try
            {
                string webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(webRootPath, relativePath);

                Console.WriteLine($"🗑️  Xóa file: {filePath}");

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"✅ Đã xóa file thành công");
                    return Ok(new { message = "Xóa file thành công" });
                }

                return NotFound(new { message = "File không tồn tại" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi xóa file: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi khi xóa file", error = ex.Message });
            }
        }
    }
}