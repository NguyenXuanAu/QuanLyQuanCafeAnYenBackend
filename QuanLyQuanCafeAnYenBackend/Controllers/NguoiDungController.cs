using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace AnYenCoffee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public NguoiDungController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpGet("TraCuu/{soDienThoai}")]
        public async Task<IActionResult> TraCuuDiem(string soDienThoai)
        {
            var khachHang = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.SoDienThoai == soDienThoai);

            if (khachHang == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng với số điện thoại này." });
            }

            string hangThanhVien = "Thành viên";
            if (khachHang.DiemTichLuy >= 500) hangThanhVien = "Vàng";
            else if (khachHang.DiemTichLuy >= 100) hangThanhVien = "Bạc";

            return Ok(new
            {
                hoTen = khachHang.HoTen,
                soDienThoai = khachHang.SoDienThoai,
                diemTichLuy = khachHang.DiemTichLuy,
                hang = hangThanhVien
            });
        }

        [HttpGet("LichSu/{soDienThoai}")]
        public async Task<IActionResult> LayLichSuDiem(string soDienThoai)
        {
            var khachHang = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.SoDienThoai == soDienThoai);

            if (khachHang == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng." });
            }

            var lichSu = await _context.LichSuDiems
                .Where(ls => ls.MaNguoiDung == khachHang.MaNguoiDung)
                .OrderByDescending(ls => ls.ThoiGian)
                .Select(ls => new {
                    ls.MaGiaoDich,
                    ls.LoaiGiaoDich,
                    ls.SoDiem,
                    ls.MoTa,
                    ls.ThoiGian
                })
                .ToListAsync();

            return Ok(lichSu);
        }

        [HttpPost("CongDiem")]
        public async Task<IActionResult> CongDiem([FromBody] CongDiem request)
        {
            var khachHang = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.SoDienThoai == request.SoDienThoai);

            if (khachHang == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng." });
            }

            khachHang.DiemTichLuy += request.SoDiem;

            // Tạo mã giao dịch
            string maGiaoDich = "LS" + DateTime.Now.Ticks.ToString().Substring(8, 8);

            var lichSu = new LichSuDiem
            {
                MaGiaoDich = maGiaoDich,
                LoaiGiaoDich = 1, 
                SoDiem = request.SoDiem,
                MoTa = request.MoTa ?? "Tích điểm mua hàng",
                ThoiGian = DateTime.Now,
                MaNguoiDung = khachHang.MaNguoiDung
            };

            _context.LichSuDiems.Add(lichSu);
            await _context.SaveChangesAsync();

           
            string hangThanhVien = "Thành viên";
            if (khachHang.DiemTichLuy >= 500) hangThanhVien = "Vàng";
            else if (khachHang.DiemTichLuy >= 100) hangThanhVien = "Bạc";

            return Ok(new
            {
                message = "Cộng điểm thành công!",
                diemMoi = khachHang.DiemTichLuy,
                hangMoi = hangThanhVien
            });

        }
        [HttpPost("DungDiem")]
        public async Task<IActionResult> DungDiem([FromBody] CongDiem request)
        {
            var khachHang = await _context.NguoiDungs
                .FirstOrDefaultAsync(nd => nd.SoDienThoai == request.SoDienThoai);

            if (khachHang == null) return NotFound(new { Message = "Không tìm thấy khách hàng." });

            if (khachHang.DiemTichLuy < request.SoDiem)
            {
                return BadRequest(new { Message = "Khách hàng không đủ điểm để thực hiện giao dịch này." });
            }

            // Thực hiện trừ điểm
            khachHang.DiemTichLuy -= request.SoDiem;

            string maGiaoDich = "SD" + DateTime.Now.Ticks.ToString().Substring(8, 8); 

            var lichSu = new LichSuDiem
            {
                MaGiaoDich = maGiaoDich,
                LoaiGiaoDich = 0, 
                SoDiem = request.SoDiem,
                MoTa = request.MoTa ?? "Sử dụng điểm đổi ưu đãi",
                ThoiGian = DateTime.Now,
                MaNguoiDung = khachHang.MaNguoiDung
            };

            _context.LichSuDiems.Add(lichSu);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Sử dụng điểm thành công!",
                DiemMoi = khachHang.DiemTichLuy
            });
        }
    }
}