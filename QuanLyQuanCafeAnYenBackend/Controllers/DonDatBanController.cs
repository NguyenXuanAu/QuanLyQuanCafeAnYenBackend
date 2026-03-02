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
        // 3. API: Lấy không gian tầng và bàn còn trống (ĐÃ FIX LỖI CS1061 CHUẨN)
        // ==============================================================
        [HttpGet("GetAvailableSpaces")]
        public async Task<IActionResult> GetAvailableSpaces([FromQuery] string? thoiGianDen)
        {
            DateTime? parsedTime = null;
            if (!string.IsNullOrEmpty(thoiGianDen) && DateTime.TryParse(thoiGianDen, out DateTime temp))
            {
                parsedTime = temp;
            }

            int thoiGianNgoiMoi = 60; // Khách mới dự kiến ngồi 60p

            var data = await _context.Tangs
                .Select(t => new {
                    t.MaTang,
                    t.TenTang,
                    t.LaKhuYenTinh,
                    t.MoTa,
                    Bans = _context.Bans
                        .Where(b => b.MaTang == t.MaTang)
                        .Select(b => new {
                            b.MaBan,
                            b.TenBan,
                            b.SoGhe,
                            b.LoaiBan,
                            b.GanCuaSo,
                            IsAvailable = parsedTime == null ? true :
                                // ĐIỀU KIỆN 1: Bàn KHÔNG trùng với Lịch Đặt Trước 
                                // (ThoiGianDen là DateTime thường nên KHÔNG CẦN .Value)
                                !_context.DonDatBans.Any(d =>
                                    d.MaBan == b.MaBan && d.TrangThai < 3 &&
                                    d.ThoiGianDen < parsedTime.Value.AddMinutes(thoiGianNgoiMoi) &&
                                    d.ThoiGianDen.AddMinutes(d.ThoiGianNgoiDuKien ?? 60) > parsedTime.Value
                                )
                                &&
                                // ĐIỀU KIỆN 2: Bàn KHÔNG bị chiếm bởi khách vãng lai 
                                // (ThoiGianTao là DateTime? nên BẮT BUỘC PHẢI CÓ .Value)
                                !_context.DonHangs.Any(dh =>
                                    dh.MaBan == b.MaBan &&
                                    (dh.TrangThai == 0 || dh.TrangThai == 1 || dh.TrangThai == 2) &&
                                    dh.ThoiGianTao != null && // Bỏ qua đơn rỗng
                                    dh.ThoiGianTao.Value.AddMinutes(120) > parsedTime.Value
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
        // 5. API: LẤY SƠ ĐỒ BÀN CHO NHÂN VIÊN (Đã tách riêng bàn Đã Thanh Toán)
        // ==============================================================
        [HttpGet("floor-map/{maNhanVien}")]
        public async Task<IActionResult> GetFloorMap(string maNhanVien)
        {
            string maTang = maNhanVien switch
            {
                "NV001" => "T01",
                "NV002" => "T02",
                "NV003" => "T03",
                "NV004" => "T04",
                _ => "T01"
            };

            var danhSachBan = await _context.Bans.Where(b => b.MaTang == maTang).ToListAsync();

            // 1. Nhóm ĐANG NGỒI THẬT (Chưa thanh toán hoặc mới Cọc - Màu Đỏ)
            var donHangChuaThanhToan = await _context.DonHangs
                .Where(dh => dh.TrangThai == 0 || dh.TrangThai == 1)
                .Select(dh => dh.MaBan)
                .ToListAsync();

            // 1.5. Nhóm ĐÃ THANH TOÁN (Nhưng khách chưa về - Màu Xanh Dương)
            var donHangDaThanhToan = await _context.DonHangs
                .Where(dh => dh.TrangThai == 2)
                .Select(dh => dh.MaBan)
                .ToListAsync();

            // 2. Nhóm ĐẶT TRƯỚC (Màu Vàng) 
            var gioHienTai = DateTime.Now;
            var gioChoPhepTre = gioHienTai.AddMinutes(-30);
            var gioSapToi = gioHienTai.AddHours(2);

            var lichDatSapToi = await _context.DonDatBans
                .Where(ddb => ddb.ThoiGianDen >= gioChoPhepTre && ddb.ThoiGianDen <= gioSapToi && ddb.TrangThai < 3)
                .Select(ddb => ddb.MaBan)
                .ToListAsync();

            // 💥 KẾT HỢP DỮ LIỆU: Ưu tiên Đang ngồi (Occupied) -> Đã thanh toán (Paid) -> Đặt trước (Reserved) -> Trống
            var result = danhSachBan.Select(b => new {
                b.MaBan,
                b.TenBan,
                TrangThai = donHangChuaThanhToan.Contains(b.MaBan) ? "Occupied" :
                            (donHangDaThanhToan.Contains(b.MaBan) ? "Paid" :
                            (lichDatSapToi.Contains(b.MaBan) ? "Reserved" : "Available"))
            });

            return Ok(result);
        }
    }
}