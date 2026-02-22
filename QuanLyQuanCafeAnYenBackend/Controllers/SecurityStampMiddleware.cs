using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
using System.Security.Claims;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    public class SecurityStampMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityStampMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, QuanLyQuanCafeDbContext dbContext)
        {
            // 1. Lấy thông tin từ Token người dùng gửi lên
            var user = context.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var staffId = user.FindFirst("StaffId")?.Value;
                var stampFromToken = user.FindFirst("SecurityStamp")?.Value;

                if (!string.IsNullOrEmpty(staffId))
                {
                    // 2. So khớp với Stamp hiện tại trong Database`    
                    var staff = await dbContext.NhanViens
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.MaNhanVien.Trim() == staffId.Trim());

                    // 3. Nếu lệch Stamp hoặc User bị khóa -> Chặn lại ngay (401)
                    if (staff == null ||
                        (staff.SecurityStamp?.Trim() != stampFromToken?.Trim()) ||
                        staff.TrangThai == false)
                    {
                        context.Response.StatusCode = 401; // Trả về lỗi Unauthorized
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Phiên làm việc hết hạn hoặc tài khoản đã bị khóa!"
                        });
                        return;
                    }
                }
            }

            // Nếu mọi thứ hợp lệ, cho phép đi tiếp đến Controller
            await _next(context);
        }
    }
}