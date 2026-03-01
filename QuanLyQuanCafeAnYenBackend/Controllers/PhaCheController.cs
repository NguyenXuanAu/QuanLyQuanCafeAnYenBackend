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
        // 1. API LẤY DANH SÁCH "ĐỢT" PHA CHẾ (Đã dùng LINQ JOIN để sửa lỗi Model)
        // ==============================================================
        [HttpGet("DanhSach")]
        public async Task<IActionResult> GetDanhSachPhaChe()
        {
            try
            {
                var baNgayTruoc = DateTime.Now.AddDays(-1);
                // Dùng cú pháp JOIN rõ ràng để kết nối 3 bảng: ChiTiet, DonHang, MonAn
                var danhSachBatches = await (from ct in _context.ChiTietDonHangs
                                             join dh in _context.DonHangs on ct.MaDonHang equals dh.MaDonHang
                                             join ma in _context.MonAns on ct.MaMonAn equals ma.MaMonAn
                                             where ct.TrangThaiBep >= 0 && ct.TrangThaiBep <= 3
                                                && dh.ThoiGianTao >= baNgayTruoc // Chỉ lấy món chưa bưng
                                             // Nhóm các món theo Đơn Hàng và Trạng Thái
                                             group new { ct, ma } by new { dh.MaDonHang, ct.TrangThaiBep, dh.MaBan, dh.ThoiGianTao, dh.GhiChu } into g
                                             select new
                                             {
                                                 // 💥 BÍ QUYẾT: Tạo Mã Ảo (Ví dụ: DH001_0)
                                                 MaDonHang = g.Key.MaDonHang + "_" + g.Key.TrangThaiBep,
                                                 MaBan = g.Key.MaBan,
                                                 ThoiGianTao = g.Key.ThoiGianTao,
                                                 // Dịch TrangThaiBep (0->3) sang LoaiDonHang (1->4)
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

                // Lấy tên bàn
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
        // 2. API CẬP NHẬT TRẠNG THÁI CỦA 1 "ĐỢT" MÓN ĂN
        // ==============================================================
        [HttpPost("CapNhatTrangThai/{maVirtual}")]
        public async Task<IActionResult> CapNhatTrangThai(string maVirtual, [FromBody] int trangThaiMoiFE)
        {
            try
            {
                // Tách Mã Ảo "DH001_0" thành "DH001" và "0"
                var parts = maVirtual.Split('_');
                if (parts.Length != 2) return BadRequest(new { success = false, message = "Mã đơn không hợp lệ" });

                string maDonHangGoc = parts[0];
                int trangThaiBepCu = int.Parse(parts[1]);
                int trangThaiBepMoi = trangThaiMoiFE - 1; // Dịch ngược từ 1->4 về 0->3

                // Chỉ tìm và cập nhật CÁC MÓN TRONG ĐỢT ĐÓ, không đụng tới món khác của đơn hàng
                var listMonAnTrongDot = await _context.ChiTietDonHangs
                    .Where(ct => ct.MaDonHang == maDonHangGoc && ct.TrangThaiBep == trangThaiBepCu)
                    .ToListAsync();

                if (!listMonAnTrongDot.Any())
                    return NotFound(new { success = false, message = "Không tìm thấy món ăn nào trong đợt này. Có thể đã được cập nhật bởi người khác!" });

                foreach (var item in listMonAnTrongDot)
                {
                    item.TrangThaiBep = trangThaiBepMoi;
                }

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