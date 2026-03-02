using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NhacVoteController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public NhacVoteController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpPost("vote")]
        public async Task<IActionResult> Vote([FromBody] NhacVote model)
        {
            if (string.IsNullOrEmpty(model.MaNguoiDung))
                return Unauthorized("Bạn chưa đăng nhập");

            var existed = await _context.NhacVote
                .AnyAsync(x => x.MaYeuCau == model.MaYeuCau
                            && x.MaNguoiDung == model.MaNguoiDung);

            if (existed)
                return BadRequest("Bạn đã vote bài này rồi");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.NhacVote.Add(new NhacVote
                {
                    MaYeuCau = model.MaYeuCau,
                    MaNguoiDung = model.MaNguoiDung
                });

                var nhac = await _context.YeuCauNhacs
                    .FirstOrDefaultAsync(x => x.MaYeuCau == model.MaYeuCau);

                if (nhac == null)
                    return NotFound("Không tìm thấy bài nhạc");

                nhac.SoLuotBinhChon += 1;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Vote thành công" });
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Có lỗi xảy ra");
            }
        }
    }
}