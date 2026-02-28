using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonDatBanController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public DonDatBanController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // 1. API: Khách hàng gửi đơn đặt bàn mới
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] DonDatBan model)
        {
            // 1. Xóa các trường liên kết bảng ra khỏi danh sách kiểm tra lỗi
            ModelState.Remove("MaBanNavigation");
            ModelState.Remove("MaNguoiDungNavigation");
            ModelState.Remove("MaNhanVienXuLyNavigation");
            ModelState.Remove("MaDonDat"); // Vì mã này bạn tự sinh ở dưới

            // 2. Sau khi đã xóa các trường trên, mới kiểm tra xem dữ liệu còn lại có hợp lệ không
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Nếu vẫn lỗi 400, Swagger sẽ chỉ ra chính xác trường nào còn thiếu
            }

            try
            {
                // Gán mã đơn tự sinh
                model.MaDonDat = "DDB" + DateTime.Now.Ticks.ToString().Substring(10, 7);
                model.ThoiGianTao = DateTime.Now;
                model.TrangThai = 0;

                _context.DonDatBans.Add(model);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, maDon = model.MaDonDat });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // 2. API: Lấy danh sách đặt bàn (Dùng cho trang Admin)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DonDatBans
                .Include(x => x.MaBanNavigation) // Lấy thêm thông tin bàn để admin xem
                .OrderByDescending(x => x.ThoiGianTao)
                .ToListAsync();
            return Ok(data);
        }

        // 3. API: Lấy không gian tầng và bàn còn trống
        [HttpGet("GetAvailableSpaces")]
        public async Task<IActionResult> GetAvailableSpaces([FromQuery] string? thoiGianDen)
        {
            DateTime? parsedTime = null;

            // Nếu Frontend có gửi giờ lên, thì mới kiểm tra
            if (!string.IsNullOrEmpty(thoiGianDen) && DateTime.TryParse(thoiGianDen, out DateTime temp))
            {
                parsedTime = temp;
            }

            // Thời gian ngồi dự kiến mặc định (Ví dụ: 60 phút)
            int thoiGianNgoiMoi = 60;

            var data = await _context.Tangs
                .Select(t => new {
                    t.MaTang,
                    t.TenTang,
                    t.LaKhuYenTinh,
                    t.MoTa,
                    Bans = _context.Bans
                        .Where(b => b.MaTang == t.MaTang) // Lấy hết tất cả bàn, không lọc TrangThai = 0 nữa
                        .Select(b => new {
                            b.MaBan,
                            b.TenBan,
                            b.SoGhe,
                            b.LoaiBan,
                            b.GanCuaSo,
                            // THUẬT TOÁN KIỂM TRA TRÙNG GIỜ
                            IsAvailable = parsedTime == null ? true : !_context.DonDatBans.Any(d =>
                                d.MaBan == b.MaBan &&
                                d.TrangThai < 3 && // Bỏ qua các đơn đã Hủy (Trạng thái 3)
                                                   // Bắt đầu trùng: Giờ cũ đến TRƯỚC khi khách mới về
                                d.ThoiGianDen < parsedTime.Value.AddMinutes(thoiGianNgoiMoi) &&
                                // Kết thúc trùng: Giờ cũ về SAU khi khách mới đến
                                d.ThoiGianDen.AddMinutes(d.ThoiGianNgoiDuKien ?? 60) > parsedTime.Value
                            )
                        }).ToList()
                }).ToListAsync();

            return Ok(data);
        }




        // nhân viên đăng nhập xong sẽ được trả lại danh bàn theo tầng của nhân viên đó
        [HttpGet("floor-map/{maNhanVien}")]
        public async Task<IActionResult> GetFloorMap(string maNhanVien)
        {
            // Logic gắn cứng tầng theo mã nhân viên
            string maTang = maNhanVien switch
            {
                "NV001" => "T01",
                "NV002" => "T02",
                "NV003" => "T03",
                "NV004" => "T04",
                _ => "T01" // Mặc định tầng 1 nếu không khớp
            };

            // 1. Lấy danh sách bàn của tầng đó
            var danhSachBan = await _context.Bans
                .Where(b => b.MaTang == maTang)
                .ToListAsync();

            // 2. Lấy danh sách đơn hàng đang phục vụ (Màu Đỏ)
            var donHangHienTai = await _context.DonHangs
                .Where(dh => dh.TrangThai == 0) // Giả định 0 là chưa thanh toán
                .Select(dh => dh.MaBan)
                .ToListAsync();

            // 3. Lấy lịch đặt bàn trong 1 tiếng tới (Màu Vàng)
            var gioSapToi = DateTime.Now.AddHours(1);
            var lichDatSapToi = await _context.DonDatBans
                .Where(ddb => ddb.ThoiGianDen >= DateTime.Now && ddb.ThoiGianDen <= gioSapToi && ddb.TrangThai == 0)
                .Select(ddb => ddb.MaBan)
                .ToListAsync();

            // Kết hợp dữ liệu để trả về cho FE
            var result = danhSachBan.Select(b => new {
                b.MaBan,
                b.TenBan,
                TrangThai = donHangHienTai.Contains(b.MaBan) ? "Occupied" :
                            (lichDatSapToi.Contains(b.MaBan) ? "Reserved" : "Available")
            });

            return Ok(result);
        }
    }
}