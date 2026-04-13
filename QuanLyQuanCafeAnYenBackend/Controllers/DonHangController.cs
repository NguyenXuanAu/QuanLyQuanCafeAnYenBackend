using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
using Microsoft.AspNetCore.SignalR; 
using QuanLyQuanCafeAnYenBackend.Hubs; 

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public DonHangController(QuanLyQuanCafeDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }


        // ==============================================================
        // 1. API LẤY DANH SÁCH ĐƠN HÀNG CỦA TÔI
        // ==============================================================
        [HttpGet("MyOrders/{maNguoiDung}")]
        public async Task<IActionResult> GetMyOrders(string maNguoiDung)
        {
            try
            {
                // Lấy danh sách Đơn đặt bàn của người dùng này, sắp xếp mới nhất lên đầu
                var orders = await _context.DonDatBans
                    .Where(d => d.MaNguoiDung == maNguoiDung)
                    .OrderByDescending(d => d.ThoiGianDen)
                    .Select(d => new
                    {
                        MaDonDat = d.MaDonDat,
                        ThoiGianDen = d.ThoiGianDen,
                        SoLuongNguoi = d.SoLuongNguoi,
                        TrangThai = d.TrangThai,
                        TongBill = d.TongTienDatTruoc ?? 0,
                        TienDaCoc = d.TienCoc ?? 0,
                        GhiChu = d.GhiChu
                    })
                    .ToListAsync();

                return Ok(new { success = true, data = orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==============================================================
        // 2. API HỦY ĐƠN HÀNG
        // ==============================================================
        [HttpPost("CancelOrder/{maDonDat}")]
        public async Task<IActionResult> CancelOrder(string maDonDat)
        {
            try
            {
                var donDatBan = await _context.DonDatBans.FirstOrDefaultAsync(d => d.MaDonDat == maDonDat);
                if (donDatBan == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng này." });
                }

                if (donDatBan.TrangThai == 3)
                {
                    return BadRequest(new { success = false, message = "Đơn hàng này đã bị hủy từ trước." });
                }

                // Chuyển trạng thái Đơn đặt bàn sang 3 (Đã hủy)
                donDatBan.TrangThai = 3;

                // Tìm và chuyển trạng thái Đơn hàng (món ăn) sang 3 (nếu có)
                var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonDat == maDonDat);
                if (donHang != null)
                {
                    donHang.TrangThai = 3;
                }

                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Hủy đơn hàng thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost("CreatePreOrder")]
        public async Task<IActionResult> CreatePreOrder([FromBody] PreOrderDto dto)
        {
            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(new { success = false, message = "Giỏ hàng trống!" });
            }

            // Giao dịch (Transaction): Đảm bảo NẾU CÓ LỖI XẢY RA, mọi thay đổi (đặc biệt là điểm của khách) sẽ tự động hoàn tác!
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. TÌM LẠI ĐƠN ĐẶT BÀN GỐC
                var donDatBan = await _context.DonDatBans.FirstOrDefaultAsync(d => d.MaDonDat == dto.MaDonDat);
                if (donDatBan == null)
                {
                    return BadRequest(new { success = false, message = "Không tìm thấy thông tin đặt bàn hợp lệ." });
                }

                // =========================================================
                // 💥 2. XỬ LÝ TRỪ ĐIỂM LOYALTY (Nếu khách có dùng điểm)
                // =========================================================
                decimal tienGiamGia = 0;
                if (dto.SoDiemMuonDung > 0)
                {
                    // Ưu tiên lấy mã người dùng từ Đơn Đặt Bàn, nếu rỗng thì lấy từ DTO
                    string maKhachHang = donDatBan.MaNguoiDung ?? dto.MaNguoiDung;

                    if (string.IsNullOrEmpty(maKhachHang))
                    {
                        return BadRequest(new { success = false, message = "Không xác định được tài khoản để trừ điểm." });
                    }

                    // Lấy thông tin khách hàng để kiểm tra
                    var khachHang = await _context.NguoiDungs.FirstOrDefaultAsync(nd => nd.MaNguoiDung == maKhachHang);
                    if (khachHang == null || khachHang.DiemTichLuy < dto.SoDiemMuonDung)
                    {
                        return BadRequest(new { success = false, message = "Điểm tích lũy không đủ hoặc tài khoản không hợp lệ!" });
                    }

                    // Tính tiền giảm giá (1 điểm = 1000 VNĐ)
                    tienGiamGia = dto.SoDiemMuonDung * 1000;

                    // Trừ điểm trực tiếp vào Profile khách hàng
                    khachHang.DiemTichLuy -= dto.SoDiemMuonDung;

                    // Lưu vết vào Bảng Lịch Sử Điểm để Kế toán đối soát
                    string maGiaoDich = "SD" + DateTime.Now.Ticks.ToString().Substring(8, 8);
                    var lichSu = new LichSuDiem
                    {
                        MaGiaoDich = maGiaoDich,
                        LoaiGiaoDich = 0, // 0: Dùng điểm, 1: Cộng điểm
                        SoDiem = dto.SoDiemMuonDung,
                        MoTa = $"Dùng điểm giảm giá cho đơn đặt bàn {donDatBan.MaDonDat}",
                        ThoiGian = DateTime.Now,
                        MaNguoiDung = khachHang.MaNguoiDung
                    };
                    _context.LichSuDiems.Add(lichSu);

                    // GHI CHÚ THÔNG MINH (Báo cho nhân viên biết bill bị hụt tiền là do dùng điểm)
                    string noteGiamGia = $"[Đã dùng {dto.SoDiemMuonDung} điểm: -{tienGiamGia:N0}đ]";
                    dto.GhiChu = string.IsNullOrEmpty(dto.GhiChu) ? noteGiamGia : dto.GhiChu + $" | {noteGiamGia}";
                }

                // =========================================================
                // 🚀 3. TỰ ĐỘNG PHÂN CÔNG NHÂN VIÊN THEO TẦNG (Giữ nguyên)
                // =========================================================
                string maNhanVienPhuTrach = null;
                if (!string.IsNullOrEmpty(donDatBan.MaBan))
                {
                    var thongTinBan = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == donDatBan.MaBan);

                    if (thongTinBan != null && !string.IsNullOrEmpty(thongTinBan.MaTang))
                    {
                        if (thongTinBan.MaTang.Contains("1"))
                        {
                            maNhanVienPhuTrach = "NV001";
                        }
                        else if (thongTinBan.MaTang.Contains("2"))
                        {
                            maNhanVienPhuTrach = "NV002";
                        }
                    }
                }

                // =========================================================
                // 💰 4. TÍNH TOÁN TIỀN BẠC (CÓ ÁP DỤNG TRỪ ĐIỂM)
                // =========================================================
                decimal tongTienMonAn = dto.Items.Sum(i => i.GiaTaiThoiDiem * i.SoLuong);
                decimal vat = Math.Round(tongTienMonAn * 0.1m); // VAT 10%
                decimal tongBillGoc = tongTienMonAn + vat;

                // Tổng tiền sau cùng (Dùng Math.Max để đảm bảo không bao giờ bị âm tiền nếu lỡ giảm quá lố)
                decimal tongBillThucTe = Math.Max(0, tongBillGoc - tienGiamGia);

                // 💥 Tiền cọc được tính trên TỔNG BILL THỰC TẾ (Đã trừ điểm)
                decimal soTienCanCoc = Math.Round(tongBillThucTe * dto.PhanTramCoc / 100);

                donDatBan.TongTienDatTruoc = tongBillThucTe;
                donDatBan.TienCoc = 0;

                // 5. TẠO ĐƠN HÀNG VÀ GẮN MÃ NHÂN VIÊN VÀO
                var donHang = new DonHang
                {
                    MaDonHang = "DH" + DateTime.Now.Ticks.ToString().Substring(10, 8),
                    LoaiDonHang = 1,
                    ThoiGianTao = DateTime.Now,
                    TrangThai = 0,
                    GhiChu = dto.GhiChu,
                    MaDonDat = dto.MaDonDat,
                    MaBan = donDatBan.MaBan,
                    MaNguoiDung = donDatBan.MaNguoiDung ?? dto.MaNguoiDung,
                    MaNhanVienNhan = maNhanVienPhuTrach
                };
                _context.DonHangs.Add(donHang);

                // 6. THÊM CHI TIẾT ĐƠN HÀNG
                int index = 1;
                foreach (var item in dto.Items)
                {
                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTiet = "CT" + DateTime.Now.Ticks.ToString().Substring(11, 7) + index,
                        MaDonHang = donHang.MaDonHang,
                        MaMonAn = item.MaMonAn,
                        SoLuong = item.SoLuong,
                        Size = item.Size,
                        MucDo = item.MucDo,
                        GiaTaiThoiDiem = item.GiaTaiThoiDiem, // Vẫn lưu giá gốc từng món ăn để báo cáo doanh thu không bị sai lệch
                        GhiChu = item.GhiChu,
                        TrangThaiBep = 0
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                    index++;
                }

                // 7. LƯU TOÀN BỘ VÀ CHỐT GIAO DỊCH
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lưu đơn hàng thành công!",
                    maDonDat = donDatBan.MaDonDat,
                    tongBill = tongBillThucTe,
                    soTienCanCoc = soTienCanCoc,
                    phanTramCoc = dto.PhanTramCoc
                });
            }
            catch (Exception ex)
            {
                // NẾU CÓ LỖI: Hoàn tác toàn bộ! Điểm của khách sẽ không bị trừ.
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }


        // ==============================================================
        // (ĐÃ CẬP NHẬT) API LẤY ĐƠN HÀNG HIỆN TẠI (Thêm logic lấy Tiền Cọc)
        // ==============================================================
        [HttpGet("table/{maBan}/active")]
        public async Task<IActionResult> GetActiveOrderByTable(string maBan)
        {
            // 1. Lấy đơn hàng chưa cọc (0), đã cọc (1) HOẶC đã thanh toán nhưng chưa về (2)
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaBan == maBan && (d.TrangThai == 0 || d.TrangThai == 1 || d.TrangThai == 2));
            if (donHang == null) return Ok(new { success = false, message = "Bàn trống." });

            // 2. 💥 TÌM SỐ TIỀN KHÁCH ĐÃ CỌC (Nếu đơn này đến từ đặt bàn online)
            decimal tienDaCoc = 0;
            if (!string.IsNullOrEmpty(donHang.MaDonDat))
            {
                var donDatBan = await _context.DonDatBans.FirstOrDefaultAsync(d => d.MaDonDat == donHang.MaDonDat);
                if (donDatBan != null)
                {
                    tienDaCoc = donDatBan.TienCoc ?? 0;
                }
            }

            // 3. Lấy danh sách món ăn
            var items = await _context.ChiTietDonHangs
                .Where(c => c.MaDonHang == donHang.MaDonHang)
                .Join(_context.MonAns, c => c.MaMonAn, m => m.MaMonAn, (c, m) => new {
                    c.MaChiTiet,
                    c.MaMonAn,
                    m.TenMonAn,
                    c.SoLuong,
                    c.GiaTaiThoiDiem,
                    c.GhiChu,
                    c.TrangThaiBep
                }).ToListAsync();

            return Ok(new
            {
                success = true,
                maDonHang = donHang.MaDonHang,
                trangThaiDonHang = donHang.TrangThai,
                tongTienGoc = items.Sum(i => i.SoLuong * i.GiaTaiThoiDiem),
                tienDaCoc = tienDaCoc, // 💥 TRẢ VỀ TIỀN CỌC CHO FRONTEND
                items = items
            });
        }

        // ==============================================================
        // 6. API XÁC NHẬN THANH TOÁN (Chuyển sang Trạng thái 2)
        // ==============================================================
        [HttpPost("ThanhToan/{maDonHang}")]
        public async Task<IActionResult> ThanhToan(string maDonHang)
        {
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);
            if (donHang == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            // 2: Đã thanh toán 100% (Thu đủ tiền)
            donHang.TrangThai = 2;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Thanh toán thành công! Khách vẫn đang ngồi tại bàn." });
        }

        // ==============================================================
        // 7. API GIẢI PHÓNG BÀN (Tự động hủy món thừa & Báo SignalR)
        // ==============================================================
        [HttpPost("GiaiPhongBan/{maDonHang}")]
        public async Task<IActionResult> GiaiPhongBan(string maDonHang)
        {
            // 1. Tìm đơn hàng (Hóa đơn)
            var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaDonHang == maDonHang);
            if (donHang == null) return NotFound(new { success = false, message = "Không tìm thấy đơn hàng" });

            // 2. Chuyển trạng thái hóa đơn sang 4 (Hoàn tất)
            donHang.TrangThai = 4;

            // 3. ĐỒNG BỘ ĐƠN ĐẶT BÀN (NẾU CÓ)
            if (!string.IsNullOrEmpty(donHang.MaDonDat))
            {
                var donDatBan = await _context.DonDatBans.FirstOrDefaultAsync(d => d.MaDonDat == donHang.MaDonDat);
                if (donDatBan != null) donDatBan.TrangThai = 4;
            }

            // 4. 💥 QUÉT VÀ HỦY MÓN CHƯA HOÀN THÀNH
            var monChuaXong = await _context.ChiTietDonHangs
                .Where(ct => ct.MaDonHang == maDonHang && ct.TrangThaiBep < 3)
                .ToListAsync();

            if (monChuaXong.Any())
            {
                // Chuyển toàn bộ món dang dở thành -1 (Đã hủy)
                foreach (var item in monChuaXong)
                {
                    item.TrangThaiBep = -1;
                }

                // Lấy tên bàn để phóng loa
                string tenBan = "Mang đi";
                if (!string.IsNullOrEmpty(donHang.MaBan))
                {
                    var ban = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == donHang.MaBan);
                    if (ban != null) tenBan = ban.TenBan;
                }

                // 💥 BẮN THÔNG BÁO CHO BẾP BIẾT ĐỂ NGỪNG PHA CHẾ
                await _hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", new
                {
                    msg = $"🛑 Bàn {tenBan} đã đi về. Bếp vui lòng NGỪNG LÀM các món còn lại nhé!",
                    orderId = maDonHang,
                    tableName = tenBan
                });
            }

            // 5. Lưu tất cả thay đổi
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Đã dọn bàn và nhả chỗ thành công!" });
        }

        // ==============================================================
        // 5. API NHẬN ĐƠN (HỖ TRỢ CẢ TẠO MỚI LẪN GỌI THÊM)
        // ==============================================================
        [HttpPost("staff-order")]
        public async Task<IActionResult> CreateStaffOrder([FromBody] StaffOrderRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return BadRequest(new { success = false, message = "Chưa chọn món nào!" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // KIỂM TRA: Bàn này đang trống hay đã có khách? (Lấy cả khách đang nợ, đã cọc, hoặc đã thanh toán 100%)
                var donHang = await _context.DonHangs.FirstOrDefaultAsync(d => d.MaBan == request.MaBan && (d.TrangThai == 0 || d.TrangThai == 1 || d.TrangThai == 2));
                string maDonHang = "";

                if (donHang == null)
                {
                    // Trạng thái 1: BÀN TRỐNG -> Tạo Đơn Hàng Mới
                    maDonHang = "DH" + DateTime.Now.Ticks.ToString().Substring(10, 8);
                    donHang = new DonHang
                    {
                        MaDonHang = maDonHang,
                        LoaiDonHang = 1,
                        ThoiGianTao = DateTime.Now,
                        TrangThai = 0,
                        MaBan = request.MaBan,
                        MaNhanVienNhan = request.MaNhanVien,
                        GhiChu = "Order tại bàn"
                    };
                    _context.DonHangs.Add(donHang);
                }
                else
                {
                    // Trạng thái 2: BÀN CÓ KHÁCH -> Dùng lại mã Đơn Hàng Cũ
                    maDonHang = donHang.MaDonHang;

                    // 💥 NẾU KHÁCH ĐÃ THANH TOÁN 100% (2) MÀ GỌI THÊM MÓN -> ĐƯA VỀ LẠI TRẠNG THÁI CHƯA THANH TOÁN (0)
                    if (donHang.TrangThai == 2)
                    {
                        donHang.TrangThai = 0;
                    }
                }

                // Thêm các món MỚI vào Chi Tiết Đơn Hàng
                int index = 1;
                foreach (var item in request.Items)
                {
                    var chiTiet = new ChiTietDonHang
                    {
                        MaChiTiet = "CT" + DateTime.Now.Ticks.ToString().Substring(11, 7) + index,
                        SoLuong = item.SoLuong,
                        GiaTaiThoiDiem = item.GiaTien,
                        GhiChu = item.GhiChu,
                        TrangThaiBep = 0, // 0: Đợi pha chế (Bếp sẽ chỉ thấy món mới này nảy số)
                        MaDonHang = maDonHang,
                        MaMonAn = item.MaMonAn
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                    index++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { success = true, message = "Đã gửi đơn xuống bếp thành công!" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Lỗi server: " + ex.Message });
            }
        }
    }
    public class StaffOrderRequest
    {
        public string MaBan { get; set; }
        public string MaNhanVien { get; set; }
        public List<StaffOrderItem> Items { get; set; }
    }

    public class StaffOrderItem
    {
        public string MaMonAn { get; set; }
        public int SoLuong { get; set; }
        public decimal GiaTien { get; set; }
        public string GhiChu { get; set; }
    }

}