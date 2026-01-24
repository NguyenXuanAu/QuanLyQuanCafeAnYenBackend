using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TangController : Controller
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public TangController(QuanLyQuanCafeDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ========================================
        // GET METHODS
        // ========================================

        [HttpGet]
        public IActionResult GetAllTang()
        {
            var tangs = _context.Tangs
                .Include(t => t.HinhAnhTangs)
                .Select(t => new
                {
                    t.MaTang,
                    t.TenTang,
                    t.LaKhuYenTinh,
                    t.MoTa,
                    t.ChuThich,
                    HinhAnhs = t.HinhAnhTangs.OrderBy(h => h.ThuTuHienThi).Select(h => new
                    {
                        h.MaHinhAnh,
                        h.DuongDanAnh,
                        h.LaAnhDaiDien,
                        h.ThuTuHienThi,
                        h.ChuThich
                    }).ToList()
                })
                .ToList();

            return Ok(new { message = "Lấy danh sách tầng thành công", data = tangs });
        }

        [HttpGet("{id}")]
        public IActionResult GetTangById(string id)
        {
            var tang = _context.Tangs
                .Include(t => t.HinhAnhTangs)
                .Where(t => t.MaTang == id)
                .Select(t => new
                {
                    t.MaTang,
                    t.TenTang,
                    t.LaKhuYenTinh,
                    t.MoTa,
                    t.ChuThich,
                    HinhAnhs = t.HinhAnhTangs.OrderBy(h => h.ThuTuHienThi).Select(h => new
                    {
                        h.MaHinhAnh,
                        h.DuongDanAnh,
                        h.LaAnhDaiDien,
                        h.ThuTuHienThi,
                        h.ChuThich
                    }).ToList()
                })
                .FirstOrDefault();

            if (tang == null) return NotFound(new { message = "Tầng không tồn tại" });

            return Ok(new { message = "Lấy thông tin tầng thành công", data = tang });
        }

        [HttpGet("Search")]
        public IActionResult SearchTang([FromQuery] string keyword)
        {
            var tang = _context.Tangs
                .Include(t => t.HinhAnhTangs)
                .Where(t => t.TenTang.Contains(keyword) ||
                            t.MaTang.Contains(keyword) ||
                            (t.MoTa != null && t.MoTa.Contains(keyword)))
                .Select(t => new
                {
                    t.MaTang,
                    t.TenTang,
                    t.LaKhuYenTinh,
                    t.MoTa,
                    t.ChuThich,
                    HinhAnhs = t.HinhAnhTangs.OrderBy(h => h.ThuTuHienThi).Select(h => new
                    {
                        h.MaHinhAnh,
                        h.DuongDanAnh,
                        h.LaAnhDaiDien,
                        h.ThuTuHienThi,
                        h.ChuThich
                    }).ToList()
                })
                .ToList();

            return Ok(new { message = "Tìm kiếm tầng thành công", data = tang });
        }

        // ========================================
        // CREATE METHOD
        // ========================================

        [HttpPost]
        public IActionResult CreateTang([FromBody] TangCreateDTO request)
        {
            if (_context.Tangs.Any(t => t.MaTang == request.MaTang))
            {
                return BadRequest(new { message = "Mã tầng đã tồn tại" });
            }

            var newTang = new Tang
            {
                MaTang = request.MaTang,
                TenTang = request.TenTang,
                LaKhuYenTinh = request.LaKhuYenTinh ?? false,
                MoTa = request.MoTa,
                ChuThich = request.ChuThich
            };

            _context.Tangs.Add(newTang);
            _context.SaveChanges();

            return Ok(new { message = "Tạo tầng mới thành công", data = newTang });
        }

        // ========================================
        // UPDATE METHODS
        // ========================================

        [HttpPut("{id}")]
        public IActionResult UpdateTang(string id, [FromBody] TangUpdateDTO updatedTang)
        {
            var tang = _context.Tangs.FirstOrDefault(t => t.MaTang == id);
            if (tang == null) return NotFound(new { message = "Tầng không tồn tại" });

            tang.TenTang = updatedTang.TenTang ?? tang.TenTang;
            tang.LaKhuYenTinh = updatedTang.LaKhuYenTinh ?? tang.LaKhuYenTinh;
            tang.MoTa = updatedTang.MoTa ?? tang.MoTa;
            tang.ChuThich = updatedTang.ChuThich;

            _context.SaveChanges();

            return Ok(new { message = "Cập nhật thông tin tầng thành công", data = tang });
        }

        // Cập nhật tầng kèm hình ảnh - FIXED VERSION
        [HttpPut("{id}/FullUpdate")]
        public IActionResult UpdateTangWithImages(string id, [FromBody] TangFullUpdateDTO request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Console.WriteLine($"🔍 Bắt đầu cập nhật tầng: {id}");
                Console.WriteLine($"📝 Request data: TenTang={request.TenTang}, HinhAnhs Count={request.HinhAnhs?.Count ?? 0}");

                var tang = _context.Tangs
                    .Include(t => t.HinhAnhTangs)
                    .FirstOrDefault(t => t.MaTang == id);

                if (tang == null)
                {
                    Console.WriteLine($"❌ Không tìm thấy tầng: {id}");
                    return NotFound(new { message = "Tầng không tồn tại" });
                }

                // 1. Update thông tin tầng
                tang.TenTang = request.TenTang ?? tang.TenTang;
                tang.LaKhuYenTinh = request.LaKhuYenTinh ?? tang.LaKhuYenTinh;
                tang.MoTa = request.MoTa ?? tang.MoTa;
                tang.ChuThich = request.ChuThich ?? tang.ChuThich;

                Console.WriteLine($"✅ Đã cập nhật thông tin tầng");

                // 2. Update hình ảnh
                if (request.HinhAnhs != null && request.HinhAnhs.Count > 0)
                {
                    Console.WriteLine($"📷 Bắt đầu xử lý {request.HinhAnhs.Count} hình ảnh");

                    // Lấy danh sách ảnh cũ để so sánh
                    var oldImages = tang.HinhAnhTangs.ToList();
                    Console.WriteLine($"📷 Có {oldImages.Count} ảnh cũ trong DB");

                    // Xóa hết ảnh cũ trong DB
                    _context.HinhAnhTangs.RemoveRange(tang.HinhAnhTangs);
                    _context.SaveChanges();
                    Console.WriteLine($"✅ Đã xóa {oldImages.Count} ảnh cũ khỏi DB");

                    // Thêm ảnh mới
                    int thuTu = 1;
                    foreach (var hinhAnh in request.HinhAnhs)
                    {
                        var cleanPath = CleanPath(hinhAnh.DuongDanAnh);
                        Console.WriteLine($"   - Thêm ảnh: {cleanPath} (Đại diện: {hinhAnh.LaAnhDaiDien})");

                        var newImage = new HinhAnhTang
                        {
                            MaHinhAnh = GenerateMaHinhAnh(),
                            DuongDanAnh = cleanPath,
                            LaAnhDaiDien = hinhAnh.LaAnhDaiDien,
                            ThuTuHienThi = hinhAnh.ThuTuHienThi ?? thuTu++,
                            MaTang = id,
                            ChuThich = hinhAnh.ChuThich
                        };
                        _context.HinhAnhTangs.Add(newImage);
                    }

                    _context.SaveChanges();
                    Console.WriteLine($"✅ Đã thêm {request.HinhAnhs.Count} ảnh mới vào DB");

                    transaction.Commit();
                    Console.WriteLine($"✅ Transaction commit thành công");

                    // 3. Xóa file vật lý thừa (Safe mode) - CHẠY SAU KHI COMMIT
                    var newPaths = request.HinhAnhs.Select(x => CleanPath(x.DuongDanAnh)).ToHashSet();
                    foreach (var oldImg in oldImages)
                    {
                        if (!newPaths.Contains(oldImg.DuongDanAnh))
                        {
                            Console.WriteLine($"🗑️  Xóa file cũ: {oldImg.DuongDanAnh}");
                            DeletePhysicalFileSafe(oldImg.DuongDanAnh);
                        }
                        else
                        {
                            Console.WriteLine($"♻️  Giữ lại file: {oldImg.DuongDanAnh}");
                        }
                    }
                }
                else
                {
                    // Không gửi list ảnh, chỉ update thông tin
                    Console.WriteLine($"⚠️  Không có hình ảnh để cập nhật");
                    _context.SaveChanges();
                    transaction.Commit();
                }

                Console.WriteLine($"✅ Hoàn thành cập nhật tầng {id}");

                // Lấy lại dữ liệu mới để trả về
                var updatedTang = _context.Tangs
                    .Include(t => t.HinhAnhTangs)
                    .Where(t => t.MaTang == id)
                    .Select(t => new
                    {
                        t.MaTang,
                        t.TenTang,
                        t.LaKhuYenTinh,
                        t.MoTa,
                        t.ChuThich,
                        HinhAnhs = t.HinhAnhTangs.OrderBy(h => h.ThuTuHienThi).Select(h => new
                        {
                            h.MaHinhAnh,
                            h.DuongDanAnh,
                            h.LaAnhDaiDien,
                            h.ThuTuHienThi,
                            h.ChuThich
                        }).ToList()
                    })
                    .FirstOrDefault();

                return Ok(new
                {
                    message = "Cập nhật tầng và hình ảnh thành công",
                    data = updatedTang
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"❌ Lỗi cập nhật: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                Console.WriteLine($"❌ Inner exception: {ex.InnerException?.Message}");

                return StatusCode(500, new
                {
                    message = "Lỗi server khi cập nhật tầng",
                    error = ex.Message,
                    detail = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // ========================================
        // QUẢN LÝ HÌNH ẢNH
        // ========================================

        [HttpPost("{id}/HinhAnh")]
        public IActionResult AddImageToTang(string id, [FromBody] HinhAnhDTO hinhAnh)
        {
            var tang = _context.Tangs.FirstOrDefault(t => t.MaTang == id);
            if (tang == null) return NotFound(new { message = "Tầng không tồn tại" });

            var newImage = new HinhAnhTang
            {
                MaHinhAnh = GenerateMaHinhAnh(),
                DuongDanAnh = CleanPath(hinhAnh.DuongDanAnh),
                LaAnhDaiDien = hinhAnh.LaAnhDaiDien,
                ThuTuHienThi = hinhAnh.ThuTuHienThi ?? 0,
                MaTang = id,
                ChuThich = hinhAnh.ChuThich
            };

            _context.HinhAnhTangs.Add(newImage);
            _context.SaveChanges();

            return Ok(new { message = "Thêm hình ảnh thành công", data = newImage });
        }

        [HttpPut("HinhAnh/{maHinhAnh}")]
        public IActionResult UpdateImage(string maHinhAnh, [FromBody] HinhAnhUpdateDTO updatedImage)
        {
            var image = _context.HinhAnhTangs.FirstOrDefault(h => h.MaHinhAnh == maHinhAnh);
            if (image == null) return NotFound(new { message = "Hình ảnh không tồn tại" });

            if (!string.IsNullOrEmpty(updatedImage.DuongDanAnh) && updatedImage.DuongDanAnh != image.DuongDanAnh)
            {
                DeletePhysicalFileSafe(image.DuongDanAnh);
                image.DuongDanAnh = CleanPath(updatedImage.DuongDanAnh);
            }

            image.LaAnhDaiDien = updatedImage.LaAnhDaiDien ?? image.LaAnhDaiDien;
            image.ThuTuHienThi = updatedImage.ThuTuHienThi ?? image.ThuTuHienThi;
            image.ChuThich = updatedImage.ChuThich ?? image.ChuThich;

            _context.SaveChanges();

            return Ok(new { message = "Cập nhật hình ảnh thành công", data = image });
        }

        [HttpDelete("HinhAnh/{maHinhAnh}")]
        public IActionResult DeleteImage(string maHinhAnh)
        {
            var image = _context.HinhAnhTangs.FirstOrDefault(h => h.MaHinhAnh == maHinhAnh);
            if (image == null) return NotFound(new { message = "Hình ảnh không tồn tại" });

            DeletePhysicalFileSafe(image.DuongDanAnh);

            _context.HinhAnhTangs.Remove(image);
            _context.SaveChanges();

            return Ok(new { message = "Xóa hình ảnh thành công" });
        }

        [HttpPut("{maTang}/HinhAnh/{maHinhAnh}/SetDaiDien")]
        public IActionResult SetAnhDaiDien(string maTang, string maHinhAnh)
        {
            var oldDaiDien = _context.HinhAnhTangs
                .Where(h => h.MaTang == maTang && h.LaAnhDaiDien == true)
                .ToList();

            foreach (var img in oldDaiDien) img.LaAnhDaiDien = false;

            var newDaiDien = _context.HinhAnhTangs
                .FirstOrDefault(h => h.MaHinhAnh == maHinhAnh && h.MaTang == maTang);

            if (newDaiDien == null) return NotFound(new { message = "Hình ảnh không tồn tại" });

            newDaiDien.LaAnhDaiDien = true;
            _context.SaveChanges();

            return Ok(new { message = "Đặt ảnh đại diện thành công", data = newDaiDien });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTang(string id)
        {
            var tang = _context.Tangs.Include(t => t.HinhAnhTangs).FirstOrDefault(t => t.MaTang == id);

            if (tang == null) return NotFound(new { message = "Tầng không tồn tại" });

            foreach (var img in tang.HinhAnhTangs)
            {
                DeletePhysicalFileSafe(img.DuongDanAnh);
            }

            _context.Tangs.Remove(tang);
            _context.SaveChanges();

            return Ok(new { message = "Xóa tầng thành công" });
        }

        // ========================================
        // HELPER METHODS
        // ========================================

        private string GenerateMaHinhAnh()
        {
            return $"HAT{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
        }

        private string CleanPath(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            // Nếu là data URI, giữ nguyên
            if (url.StartsWith("data:")) return url;

            // Nếu là full URL, chỉ lấy phần path
            if (url.Contains("://"))
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }

            // Nếu có /images/, lấy từ đó trở đi
            if (url.Contains("/images/"))
            {
                var index = url.IndexOf("/images/");
                return url.Substring(index);
            }

            // Đảm bảo bắt đầu bằng /
            return url.StartsWith("/") ? url : "/" + url;
        }

        private void DeletePhysicalFileSafe(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;
            if (fileUrl.StartsWith("data:")) return; // Không xóa data URI

            try
            {
                var relativePath = CleanPath(fileUrl).TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var filePath = Path.Combine(webRootPath, relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    Console.WriteLine($"🗑️  Đã xóa file: {filePath}");
                }
                else
                {
                    Console.WriteLine($"⚠️  File không tồn tại: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Lỗi xóa file: {ex.Message}");
            }
        }
    }

    // ========================================
    // DTOs
    // ========================================
    public class TangCreateDTO
    {
        public string MaTang { get; set; } = string.Empty;
        public string TenTang { get; set; } = string.Empty;
        public bool? LaKhuYenTinh { get; set; }
        public string? MoTa { get; set; }
        public string? ChuThich { get; set; }
    }

    public class TangUpdateDTO
    {
        public string? TenTang { get; set; }
        public bool? LaKhuYenTinh { get; set; }
        public string? MoTa { get; set; }
        public string? ChuThich { get; set; }
    }

    public class TangFullUpdateDTO
    {
        public string? TenTang { get; set; }
        public bool? LaKhuYenTinh { get; set; }
        public string? MoTa { get; set; }
        public string? ChuThich { get; set; }
        public List<HinhAnhDTO>? HinhAnhs { get; set; }
    }

    public class HinhAnhDTO
    {
        public string DuongDanAnh { get; set; } = string.Empty;
        public bool LaAnhDaiDien { get; set; }
        public int? ThuTuHienThi { get; set; }
        public string? ChuThich { get; set; }
    }

    public class HinhAnhUpdateDTO
    {
        public string? DuongDanAnh { get; set; }
        public bool? LaAnhDaiDien { get; set; }
        public int? ThuTuHienThi { get; set; }
        public string? ChuThich { get; set; }
    }
}