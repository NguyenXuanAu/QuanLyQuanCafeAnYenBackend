using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR; // Thêm thư viện SignalR
using QuanLyQuanCafeAnYenBackend.Models;
using QuanLyQuanCafeAnYenBackend.Hubs; // Thêm Hubs
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhaCheController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext; // Khai báo Hub

        // Tiêm Hub vào Constructor
        public PhaCheController(QuanLyQuanCafeDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ==============================================================
        // 1. API LẤY DANH SÁCH "ĐỢT" PHA CHẾ (Giữ nguyên của bạn)
        // ==============================================================
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSachPhaChe()
        {
            try
            {
                var baNgayTruoc = DateTime.Now.AddDays(-1);
                var danhSachBatches = await (from ct in _context.ChiTietDonHangs
                                             join dh in _context.DonHangs on ct.MaDonHang equals dh.MaDonHang
                                             join ma in _context.MonAns on ct.MaMonAn equals ma.MaMonAn
                                             where ct.TrangThaiBep >= 0 && ct.TrangThaiBep <= 3
                                                && dh.ThoiGianTao >= baNgayTruoc
                                             group new { ct, ma } by new { dh.MaDonHang, ct.TrangThaiBep, dh.MaBan, dh.ThoiGianTao, dh.GhiChu } into g
                                             select new
                                             {
                                                 MaDonHang = g.Key.MaDonHang + "_" + g.Key.TrangThaiBep,
                                                 MaBan = g.Key.MaBan,
                                                 ThoiGianTao = g.Key.ThoiGianTao,
                                                 LoaiDonHang = g.Key.TrangThaiBep + 1,
                                                 GhiChu = g.Key.GhiChu,
                                                 ChiTiet = g.Select(x => new
                                                 {
                                                     MaChiTiet = x.ct.MaChiTiet,
                                                     MaMonAn = x.ct.MaMonAn,
                                                     TenMonAn = x.ma.TenMonAn,
                                                     SoLuong = x.ct.SoLuong,
                                                     Size = x.ct.Size,
                                                     MucDo = x.ct.MucDo,
                                                     GhiChu = x.ct.GhiChu
                                                 }).ToList()
                                             }).ToListAsync();

                var dsBan = await _context.Bans.ToDictionaryAsync(b => b.MaBan, b => b.TenBan);
                var finalResult = danhSachBatches.Select(b => new {
                    b.MaDonHang,
                    b.MaBan,
                    TenBan = !string.IsNullOrEmpty(b.MaBan) && dsBan.ContainsKey(b.MaBan) ? dsBan[b.MaBan] : "Mang đi",
                    b.ThoiGianTao,
                    b.LoaiDonHang,
                    b.GhiChu,
                    b.ChiTiet
                }).OrderBy(x => x.ThoiGianTao).ToList();

                return Ok(new { success = true, data = finalResult });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==============================================================
        // 2. API CẬP NHẬT TRẠNG THÁI (ĐÃ FIX LỖI INCLUDE & THÊM SIGNALR)
        // ==============================================================
        [HttpPost("CapNhatTrangThai/{maVirtual}")]
        public async Task<IActionResult> CapNhatTrangThai(string maVirtual, [FromBody] int trangThaiMoiFE)
        {
            try
            {
                var parts = maVirtual.Split('_');
                if (parts.Length != 2) return BadRequest(new { success = false, message = "Mã đơn không hợp lệ" });

                string maDonHangGoc = parts[0];
                int trangThaiBepCu = int.Parse(parts[1]);
                int trangThaiBepMoi = trangThaiMoiFE - 1;

                // 1. Chỉ lấy danh sách món để cập nhật (Không dùng Include nữa để né lỗi CS1061)
                var listMonAnTrongDot = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == maDonHangGoc && ct.TrangThaiBep == trangThaiBepCu)
                    .ToListAsync();

                if (!listMonAnTrongDot.Any())
                    return NotFound(new { success = false, message = "Không tìm thấy món." });

                // 2. Lấy riêng thông tin Đơn Hàng Gốc để dò ra tên Bàn
                var donHangGoc = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHangGoc);
                string maBan = donHangGoc?.MaBan ?? "";

                var ban = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == maBan);
                string tenBan = ban != null ? ban.TenBan : "Mang đi";

                // 3. Cập nhật trạng thái
                foreach (var item in listMonAnTrongDot)
                {
                    item.TrangThaiBep = trangThaiBepMoi;
                }

                await _context.SaveChangesAsync();

                // 💥 BẮN THÔNG BÁO REAL-TIME CHO NHÂN VIÊN KHI BẾP LÀM XONG (Kéo sang cột 4)
                if (trangThaiMoiFE == 4)
                {
                    string message = $"Bếp đã làm xong món của bàn {tenBan}. Vui lòng bưng lên cho khách!";
                    await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new
                    {
                        msg = message,
                        orderId = maDonHangGoc,
                        tableName = tenBan
                    });
                }

                return Ok(new { success = true, message = "Cập nhật tiến độ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==============================================================
        // 3. API BẾP HỦY 1 MÓN CỤ THỂ (HẾT NGUYÊN LIỆU)
        // ==============================================================
        [HttpPost("HuyMon/{maChiTiet}")]
        public async Task<IActionResult> HuyMon(string maChiTiet)
        {
            try
            {
                // 1. Tìm đúng cái món đó trong đợt order
                var chiTiet = await _context.ChiTietDonHangs.FirstOrDefaultAsync(ct => ct.MaChiTiet == maChiTiet);
                if (chiTiet == null) return NotFound(new { success = false, message = "Không tìm thấy món này." });

                // 2. Chuyển trạng thái thành -1 (Đã hủy). Việc này giúp hàm tính tiền tự động bỏ qua món này.
                chiTiet.TrangThaiBep = -1;

                // 3. Lấy thông tin Tên món và Tên bàn để phát loa thông báo (Né lỗi CS1061)
                var monAn = await _context.MonAns.FirstOrDefaultAsync(m => m.MaMonAn == chiTiet.MaMonAn);
                string tenMonAn = monAn != null ? monAn.TenMonAn : "Món ăn";

                var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == chiTiet.MaDonHang);
                string tenBan = "Mang đi";
                if (donHang != null && !string.IsNullOrEmpty(donHang.MaBan))
                {
                    var ban = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == donHang.MaBan);
                    if (ban != null) tenBan = ban.TenBan;
                }

                await _context.SaveChangesAsync();

                // 4. Bắn SignalR hỏa tốc cho Nhân viên
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new
                {
                    msg = $"🛑 Bếp đã HỦY món [{tenMonAn}] của bàn [{tenBan}] do hết nguyên liệu. Nhờ bạn báo khách nhé!",
                    orderId = chiTiet.MaDonHang,
                    tableName = tenBan
                });

                return Ok(new { success = true, message = "Đã hủy món và báo cho nhân viên!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==============================================================
        // 4. API BẾP BÁO LÀM LÂU (KHÔNG SỬA DATA, CHỈ PHÁT LOA)
        // ==============================================================
        [HttpPost("BaoLau/{maChiTiet}")]
        public async Task<IActionResult> BaoLau(string maChiTiet)
        {
            try
            {
                // 1. Tìm món
                var chiTiet = await _context.ChiTietDonHangs.FirstOrDefaultAsync(ct => ct.MaChiTiet == maChiTiet);
                if (chiTiet == null) return NotFound(new { success = false, message = "Không tìm thấy món này." });

                // 2. Lấy thông tin (Tương tự như trên)
                var monAn = await _context.MonAns.FirstOrDefaultAsync(m => m.MaMonAn == chiTiet.MaMonAn);
                string tenMonAn = monAn != null ? monAn.TenMonAn : "Món ăn";

                var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == chiTiet.MaDonHang);
                string tenBan = "Mang đi";
                if (donHang != null && !string.IsNullOrEmpty(donHang.MaBan))
                {
                    var ban = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == donHang.MaBan);
                    if (ban != null) tenBan = ban.TenBan;
                }

                // 3. Bắn SignalR (Không cần SaveChanges vì không thay đổi Data)
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new
                {
                    msg = $"⏳ Món [{tenMonAn}] của bàn [{tenBan}] sẽ ra hơi chậm. Nhờ bạn ra rót trà và xoa dịu khách nhé!",
                    orderId = chiTiet.MaDonHang,
                    tableName = tenBan
                });

                return Ok(new { success = true, message = "Đã gửi thông báo Delay thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}