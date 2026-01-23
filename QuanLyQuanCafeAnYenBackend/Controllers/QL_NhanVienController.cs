using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QL_NhanVienController : Controller
    {
        private readonly QuanLyQuanCafeDbContext _db;

        public QL_NhanVienController(QuanLyQuanCafeDbContext context)
        {
            _db = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NhanVien>>> DanhSachNhanVien(string? timkiem, string? chucVu)
        {
            var nv = _db.NhanViens.AsQueryable();

            if (!string.IsNullOrEmpty(timkiem))
            {
                string s = timkiem.Trim().ToLower();
                nv = nv.Where(n => n.HoTen.ToLower().Contains(s) || n.MaNhanVien.ToLower().Contains(s));
            }

            if (!string.IsNullOrEmpty(chucVu))
            {
                nv = nv.Where(n => n.ChucVu == chucVu);
            }

            return await nv.ToListAsync();
        }

        [HttpGet("{maNhanVien}")]
        public async Task<ActionResult<NhanVien>> ChiTietNhanVien(string maNhanVien)
        {
            var nhanVien = await _db.NhanViens.FindAsync(maNhanVien);
            if (nhanVien == null) return NotFound("Không tìm thấy nhân viên.");
            return nhanVien;
        }

        [HttpPost]
        public async Task<ActionResult<NhanVien>> ThemNhanVien(NhanVien nvMoi)
        {
            var danhSachLoi = new Dictionary<string, string[]>();

            if (_db.NhanViens.Any(n => n.MaNhanVien == nvMoi.MaNhanVien))
            {
                return BadRequest(new { errors = new { MaNhanVien = new[] { "Mã nhân viên này đã tồn tại trong hệ thống" } } });
            }

            if (_db.NhanViens.Any(n => n.SoDienThoai == nvMoi.SoDienThoai))
            {
                return BadRequest(new { errors = new { SoDienThoai = new[] { "Số điện thoại này đã được sử dụng" } } });
            }

            if (_db.NhanViens.Any(n => n.Email == nvMoi.Email))
            {
                return BadRequest(new { errors = new { Email = new[] { "Email này đã tồn tại" } } });
            }

            if (danhSachLoi.Count > 0)
                return BadRequest(new { errors = danhSachLoi });
            _db.NhanViens.Add(nvMoi);
            await _db.SaveChangesAsync();
            return CreatedAtAction("ChiTietNhanVien", new { maNhanVien = nvMoi.MaNhanVien }, nvMoi);
        }

        [HttpPut("{maNhanVien}")]
        public async Task<IActionResult> SuaNhanVien(string maNhanVien, NhanVien CapNhat)
        {
            var danhSachLoi = new Dictionary<string, string[]>();
            if (danhSachLoi.Count > 0)
                return BadRequest(new { errors = danhSachLoi });

            if (string.IsNullOrEmpty(maNhanVien) || !maNhanVien.Trim().Equals(CapNhat.MaNhanVien?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Mã nhân viên không khớp.");
            }

            var nhanVienHienTai = await _db.NhanViens.FindAsync(maNhanVien.Trim());
            if (nhanVienHienTai == null)
            {
                return NotFound("Không tìm thấy nhân viên có mã " + maNhanVien);
            }

            if (await _db.NhanViens.AnyAsync(n => n.SoDienThoai == CapNhat.SoDienThoai && n.MaNhanVien != maNhanVien.Trim()))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã tồn tại.");
            }

            if (!string.IsNullOrEmpty(CapNhat.Email) &&
                await _db.NhanViens.AnyAsync(n => n.Email == CapNhat.Email && n.MaNhanVien != maNhanVien.Trim()))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
            }

            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            nhanVienHienTai.HoTen = CapNhat.HoTen;
            nhanVienHienTai.SoDienThoai = CapNhat.SoDienThoai;
            nhanVienHienTai.Email = CapNhat.Email;
            nhanVienHienTai.ChucVu = CapNhat.ChucVu;
            nhanVienHienTai.NgayVaoLam = CapNhat.NgayVaoLam;
            nhanVienHienTai.TrangThai = CapNhat.TrangThai;

            if (!string.IsNullOrEmpty(CapNhat.MatKhauHash) && CapNhat.MatKhauHash != "123")
            {
                nhanVienHienTai.MatKhauHash = CapNhat.MatKhauHash;
            }

            try
            {
                await _db.SaveChangesAsync();
                return Ok("Cập nhật thành công.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        [HttpDelete("{maNhanVien}")]
        public async Task<IActionResult> XoaNhanVien(string maNhanVien)
        {
            var nhanVien = await _db.NhanViens.FindAsync(maNhanVien);
            if (nhanVien == null) return NotFound("Nhân viên không tồn tại.");

            _db.NhanViens.Remove(nhanVien);
            await _db.SaveChangesAsync();
            return Ok("Đã xóa nhân viên thành công.");
        }
    }
}