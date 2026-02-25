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
            var path = context.Request.Path.Value?.ToLower();

            // SỬA: Bổ qua kiểm tra Stamp cho tất cả các route đăng nhập (Khách và Nhân viên)
            // Thêm các path liên quan đến Admin Login và AuthStaff để tránh hiện Pop-up sai khi nhập lỗi mật khẩu
            if (path != null && (
                path.Contains("/login") ||
                path.Contains("/forgotpassword") ||
                path.Contains("/api/auth/login") ||
                path.Contains("/api/authstaff/login")
            ))
            {
                await _next(context);
                return;
            }

            var userPrincipal = context.User;
            if (userPrincipal.Identity?.IsAuthenticated == true)
            {
                var stampFromToken = userPrincipal.FindFirst("SecurityStamp")?.Value;
                var type = userPrincipal.FindFirst("Type")?.Value;

                bool isInvalid = false;

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

                if (isInvalid)
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new
                    {
                        success = false,
                        message = "Tài khoản của bạn đã được đăng nhập ở nơi khác hoặc vừa đổi mật khẩu. Vui lòng đăng nhập lại."
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}