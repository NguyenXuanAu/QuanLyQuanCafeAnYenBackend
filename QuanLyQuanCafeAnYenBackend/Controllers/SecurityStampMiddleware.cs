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
            var userPrincipal = context.User;
            if (userPrincipal.Identity?.IsAuthenticated == true)
            {
                // 1. Lấy mã định danh và dấu vân tay bảo mật từ Token
                var stampFromToken = userPrincipal.FindFirst("SecurityStamp")?.Value;
                var type = userPrincipal.FindFirst("Type")?.Value; // Để biết là Customer hay Staff

                bool isInvalid = false;

                // 2. Xử lý cho KHÁCH HÀNG (Dựa trên UserId)
                if (type == "Customer")
                {
                    var userId = userPrincipal.FindFirst("UserId")?.Value;
                    var customer = await dbContext.NguoiDungs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.MaNguoiDung.Trim() == userId.Trim());

                    if (customer == null || customer.SecurityStamp?.Trim() != stampFromToken?.Trim())
                    {
                        isInvalid = true;
                    }
                }
                // 3. Xử lý cho NHÂN VIÊN (Dựa trên StaffId)
                else if (type == "Staff")
                {
                    var staffId = userPrincipal.FindFirst("StaffId")?.Value;
                    var staff = await dbContext.NhanViens
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.MaNhanVien.Trim() == staffId.Trim());

                    if (staff == null || staff.SecurityStamp?.Trim() != stampFromToken?.Trim() || staff.TrangThai == false)
                    {
                        isInvalid = true;
                    }
                }

                // 4. Nếu Token không còn khớp với Database -> Chặn đứng ngay
                if (isInvalid)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Phiên làm việc hết hạn hoặc bạn đã đăng xuất từ thiết bị khác!"
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}