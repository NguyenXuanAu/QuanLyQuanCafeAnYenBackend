using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhaCheController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public PhaCheController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        // ==============================================================
        // 1. API LẤY DANH SÁCH ĐƠN HÀNG CHO PHA CHẾ (Từ 1 đến 4)
        // ==============================================================
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSachPhaChe()
        {
            try
            {
                // Lấy các đơn hàng có LoaiDonHang từ 1 đến 4, sắp xếp cũ nhất lên trước (Đến trước làm trước)
                var danhSach = await (from dh in _context.DonHangs
                                      where dh.LoaiDonHang >= 1 && dh.LoaiDonHang <= 4
                                      orderby dh.ThoiGianTao ascending
                                      select new
                                      {
                                          MaDonHang = dh.MaDonHang,
                                          MaBan = dh.MaBan,
                                          // Tự động dịch MaBan sang Tên Bàn cho dễ nhìn
                                          TenBan = _context.Bans.Where(b => b.MaBan == dh.MaBan).Select(b => b.TenBan).FirstOrDefault() ?? "Mang đi",
                                          ThoiGianTao = dh.ThoiGianTao,
                                          LoaiDonHang = dh.LoaiDonHang,
                                          GhiChu = dh.GhiChu,

                                          // Quét luôn danh sách món ăn của cái đơn đó
                                          ChiTiet = (from ct in _context.ChiTietDonHangs
                                                     join ma in _context.MonAns on ct.MaMonAn equals ma.MaMonAn
                                                     where ct.MaDonHang == dh.MaDonHang
                                                     select new
                                                     {
                                                         MaMonAn = ct.MaMonAn,
                                                         TenMonAn = ma.TenMonAn,
                                                         SoLuong = ct.SoLuong,
                                                         Size = ct.Size,
                                                         MucDo = ct.MucDo,
                                                         GhiChu = ct.GhiChu
                                                     }).ToList()
                                      }).ToListAsync();

                return Ok(new { success = true, data = danhSach });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ==============================================================
        // 2. API CẬP NHẬT TIẾN ĐỘ PHA CHẾ (Kéo thả Kanban)
        // ==============================================================
        [HttpPost("CapNhatTrangThai/{maDonHang}")]
        public async Task<IActionResult> CapNhatTrangThai(string maDonHang, [FromBody] int trangThaiMoi)
        {
            try
            {
                // Bảo mật: Chỉ cho phép Pha chế đẩy trạng thái từ 1 đến 4
                if (trangThaiMoi < 1 || trangThaiMoi > 4)
                {
                    return BadRequest(new { success = false, message = "Trạng thái không hợp lệ đối với quyền Pha chế!" });
                }

                var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonHang == maDonHang);

                if (donHang == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                // Cập nhật tiến độ
                donHang.LoaiDonHang = trangThaiMoi;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Cập nhật tiến độ thành công!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}