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

                // 2. TÍNH TOÁN TIỀN BẠC
                decimal tongTienMonAn = dto.Items.Sum(i => i.GiaTaiThoiDiem * i.SoLuong);
                decimal vat = Math.Round(tongTienMonAn * 0.1m); // VAT 10%
                decimal tongBill = tongTienMonAn + vat;

                // Số tiền khách CẦN chuyển khoản thực tế
                decimal soTienCanCoc = Math.Round(tongBill * dto.PhanTramCoc / 100);

                // Cập nhật tổng tiền vào Đơn đặt bàn
                donDatBan.TongTienDatTruoc = tongBill;
                donDatBan.TienCoc = 0;

                // 3. TẠO ĐƠN HÀNG (FIX BUG NULL Ở ĐÂY)
                var donHang = new DonHang
                {
                    MaDonHang = "DH" + DateTime.Now.Ticks.ToString().Substring(10, 8),
                    LoaiDonHang = 1,
                    ThoiGianTao = DateTime.Now,
                    TrangThai = 0, // 0: Chưa cọc
                    GhiChu = dto.GhiChu,
                    MaDonDat = dto.MaDonDat,

                    // 👉 LẤY THÔNG TIN TỪ ĐƠN ĐẶT BÀN CHUYỂN SANG ĐƠN HÀNG
                    MaBan = donDatBan.MaBan,
                    MaNguoiDung = donDatBan.MaNguoiDung ?? dto.MaNguoiDung
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