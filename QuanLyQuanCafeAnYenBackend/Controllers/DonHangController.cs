using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonHangController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public DonHangController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
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
                // 🚀 TỰ ĐỘNG PHÂN CÔNG NHÂN VIÊN THEO TẦNG
                // =========================================================
                string maNhanVienPhuTrach = null;
                if (!string.IsNullOrEmpty(donDatBan.MaBan))
                {
                    // Lấy thông tin cái Bàn đó để xem nó nằm ở Tầng nào
                    var thongTinBan = await _context.Bans.FirstOrDefaultAsync(b => b.MaBan == donDatBan.MaBan);

                    if (thongTinBan != null && !string.IsNullOrEmpty(thongTinBan.MaTang))
                    {
                        // Kiểm tra mã tầng. Dùng Contains để cho chắc chắn (VD: "T1", "TANG1", "01" đều bắt được)
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

                // 2. TÍNH TOÁN TIỀN BẠC
                decimal tongTienMonAn = dto.Items.Sum(i => i.GiaTaiThoiDiem * i.SoLuong);
                decimal vat = Math.Round(tongTienMonAn * 0.1m); // VAT 10%
                decimal tongBill = tongTienMonAn + vat;

                decimal soTienCanCoc = Math.Round(tongBill * dto.PhanTramCoc / 100);

                donDatBan.TongTienDatTruoc = tongBill;
                donDatBan.TienCoc = 0;

                // 3. TẠO ĐƠN HÀNG VÀ GẮN MÃ NHÂN VIÊN VÀO
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

                    // 👉 DỮ LIỆU TỰ ĐỘNG PHÂN CÔNG CHUI VÀO ĐÂY
                    MaNhanVienNhan = maNhanVienPhuTrach
                };
                _context.DonHangs.Add(donHang);

                // 4. THÊM CHI TIẾT ĐƠN HÀNG
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
                        GiaTaiThoiDiem = item.GiaTaiThoiDiem,
                        GhiChu = item.GhiChu,
                        TrangThaiBep = 0
                    };
                    _context.ChiTietDonHangs.Add(chiTiet);
                    index++;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    success = true,
                    message = "Lưu đơn hàng thành công!",
                    maDonDat = donDatBan.MaDonDat,
                    tongBill = tongBill,
                    soTienCanCoc = soTienCanCoc,
                    phanTramCoc = dto.PhanTramCoc
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}