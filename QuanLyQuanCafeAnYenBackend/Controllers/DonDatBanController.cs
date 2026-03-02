using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using QuanLyQuanCafeAnYenBackend.Models;
using QuanLyQuanCafeAnYenBackend.Hubs;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonDatBanController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DonDatBanController(QuanLyQuanCafeDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ==============================================================
        // 1. API: Khách hàng gửi đơn đặt bàn mới (ĐÃ KHÔI PHỤC)
        // ==============================================================
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

        // ==============================================================
        // 2. API: Lấy danh sách đặt bàn
        // ==============================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.DonDatBans
                .Include(x => x.MaBanNavigation)
                .OrderByDescending(x => x.ThoiGianTao)
                .ToListAsync();
            return Ok(data);
        }

        // ==============================================================
        // 3. API: Lấy không gian tầng và bàn còn trống (ĐÃ KHÔI PHỤC)
        // ==============================================================
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


        // ==============================================================
        // 4. API MỚI CỦA ĐỒNG ĐỘI: XÁC NHẬN HOÀN THÀNH VÀ THÔNG BÁO SIGNALR
        // ==============================================================
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
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new
                {
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

        // ==============================================================
        // 5. API: LẤY SƠ ĐỒ BÀN CHO NHÂN VIÊN (Đã update trạng thái mới)
        // ==============================================================
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
            // 💥 Đã update: Trạng thái 0 (Đang phục vụ), 1 (Đã cọc), 2 (Đã thanh toán 100% nhưng chưa dọn)
            var donHangHienTai = await _context.DonHangs
                .Where(dh => dh.TrangThai == 0 || dh.TrangThai == 1 || dh.TrangThai == 2)
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