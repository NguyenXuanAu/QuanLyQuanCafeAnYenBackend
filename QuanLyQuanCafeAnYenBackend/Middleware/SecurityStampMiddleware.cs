using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

namespace QuanLyQuanCafeAnYenBackend.Middleware
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
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // 🟢🟢🟢 QUAN TRỌNG: Bỏ qua TẤT CẢ các API liên quan đến đăng nhập
            if (path.Contains("/api/auth/login") ||                // Login khách
                path.Contains("/api/authstaff/login") ||           // Login staff
                path.Contains("/api/auth/forgotpassword") ||       // Quên mật khẩu khách
                path.Contains("/api/authstaff/forgotpassword") ||  // Quên mật khẩu staff
                path.Contains("/api/auth/resetpassword") ||        // Reset mật khẩu khách
                path.Contains("/api/authstaff/resetpassword") ||   // Reset mật khẩu staff
                path.Contains("/api/auth/register") ||             // Đăng ký khách
                path.Contains("/api/authstaff/register") ||        // Đăng ký staff
                path.Contains("/swagger") ||
                path.Contains(".css") ||
                path.Contains(".js") ||
                path.Contains(".png") ||
                path.Contains(".jpg") ||
                path.Contains(".ico") ||
                path == "/" ||
                path == "")
            {
                Console.WriteLine($"🟢 Bỏ qua kiểm tra: {path}");
                await _next(context);
                return;
            }

            // Kiểm tra token
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            // Nếu không có token, cho phép request đi tiếp
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                Console.WriteLine($"🟡 Không có token: {path}");
                await _next(context);
                return;
            }

            // Có token - kiểm tra SecurityStamp
            try
            {
                var userId = context.User.FindFirst("UserId")?.Value;
                var staffId = context.User.FindFirst("StaffId")?.Value;
                var type = context.User.FindFirst("Type")?.Value;
                var stampFromToken = context.User.FindFirst("SecurityStamp")?.Value;

                Console.WriteLine($"🔍 Kiểm tra: Type={type}, UserId={userId}, StaffId={staffId}");

                // Kiểm tra Customer
                if (type == "Customer" && !string.IsNullOrEmpty(userId))
                {
                    var customer = await dbContext.NguoiDungs
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.MaNguoiDung.Trim() == userId.Trim());

                    if (customer == null)
                    {
                        Console.WriteLine($"❌ Không tìm thấy customer: {userId}");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Phiên đăng nhập đã hết hạn!"
                        });
                        return;
                    }

                    if (customer.SecurityStamp?.Trim() != stampFromToken?.Trim())
                    {
                        Console.WriteLine($"❌ SecurityStamp không khớp: DB={customer.SecurityStamp}, Token={stampFromToken}");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Phiên đăng nhập đã hết hạn hoặc đã được đăng xuất từ thiết bị khác!"
                        });
                        return;
                    }

                    Console.WriteLine($"✅ Customer hợp lệ: {userId}");
                }
                // Kiểm tra Staff
                else if (type == "Staff" && !string.IsNullOrEmpty(staffId))
                {
                    var staff = await dbContext.NhanViens
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.MaNhanVien.Trim() == staffId.Trim());

                    if (staff == null)
                    {
                        Console.WriteLine($"❌ Không tìm thấy staff: {staffId}");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Phiên đăng nhập đã hết hạn!"
                        });
                        return;
                    }

                    if (staff.SecurityStamp?.Trim() != stampFromToken?.Trim() || staff.TrangThai == false)
                    {
                        Console.WriteLine($"❌ Staff không hợp lệ: {staffId}");
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            success = false,
                            message = "Phiên đăng nhập đã hết hạn hoặc tài khoản đã bị khóa!"
                        });
                        return;
                    }

                    Console.WriteLine($"✅ Staff hợp lệ: {staffId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi Middleware: {ex.Message}");
            }

            await _next(context);
        }
    }
}