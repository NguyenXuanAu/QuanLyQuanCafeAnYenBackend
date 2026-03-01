using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR; // Thêm thư viện này
using QuanLyQuanCafeAnYenBackend.Models;
using QuanLyQuanCafeAnYenBackend.Hubs; // Đảm bảo bạn đã tạo thư mục Hubs và NotificationHub

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonDatBanController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext; // Khai báo Hub

        public DonDatBanController(QuanLyQuanCafeDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; // Inject Hub vào Controller
        }

        // --- CÁC API CŨ CỦA BẠN GIỮ NGUYÊN ---
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] DonDatBan model) { /* Code cũ */ return Ok(); }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DonDatBans
                .Include(x => x.MaBanNavigation)
                .OrderByDescending(x => x.ThoiGianTao)
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("GetAvailableSpaces")]
        public async Task<IActionResult> GetAvailableSpaces([FromQuery] string? thoiGianDen) { /* Code cũ */ return Ok(); }


        // --- API MỚI: XÁC NHẬN HOÀN THÀNH VÀ THÔNG BÁO ---
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteBooking(string id)
        {
            try
            {
                // 1. Tìm đơn đặt bàn
                var booking = await _context.DonDatBans
                    .Include(x => x.MaBanNavigation)
                    .FirstOrDefaultAsync(x => x.MaDonDat == id);

                if (booking == null) return NotFound(new { message = "Không tìm thấy đơn" });

                // 2. Cập nhật trạng thái thành 2 (Đã hoàn thành/Đã nhận bàn)
                booking.TrangThai = 2;
                await _context.SaveChangesAsync();

                // 3. GỬI THÔNG BÁO REAL-TIME tới tất cả nhân viên
                string message = $"Bàn {booking.MaBanNavigation?.TenBan ?? "không tên"} đã làm xong món!";
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new {
                    msg = message,
                    orderId = id,
                    tableName = booking.MaBanNavigation?.TenBan
                });

                return Ok(new { success = true, message = "Đã xác nhận hoàn thành" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
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