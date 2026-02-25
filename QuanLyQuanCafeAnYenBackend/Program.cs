using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyQuanCafeAnYenBackend.Controllers;
using QuanLyQuanCafeAnYenBackend.Hubs;
using QuanLyQuanCafeAnYenBackend.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ================= 1. CẤU HÌNH SERVICES (Trước khi Build) =================

// Kết nối Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyQuanCafeDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT Settings
builder.Services.Configure<JWTSetting>(builder.Configuration.GetSection("AppSettings"));
var secretKey = builder.Configuration["AppSettings:SecretKey"];
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey ?? "");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddMemoryCache();
builder.Services.AddSignalR(); // Kích hoạt SignalR

// Cấu hình CORS duy nhất tại đây (Sửa lỗi image_134d61.png)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Bắt buộc để máy nhân viên nhận được tin
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // Chốt danh sách Services

// ================= 2. CẤU HÌNH MIDDLEWARE (Sau khi Build) =================

var webRootPath = app.Environment.WebRootPath;
if (string.IsNullOrEmpty(webRootPath))
{
    var currentDir = Directory.GetCurrentDirectory();
    webRootPath = currentDir.Contains("bin")
        ? Path.Combine(Directory.GetParent(currentDir)?.Parent?.Parent?.FullName ?? currentDir, "wwwroot")
        : Path.Combine(currentDir, "wwwroot");
    app.Environment.WebRootPath = webRootPath;
}
if (!Directory.Exists(webRootPath)) Directory.CreateDirectory(webRootPath);
var imagesPath = Path.Combine(webRootPath, "images");
if (!Directory.Exists(imagesPath)) Directory.CreateDirectory(imagesPath);
foreach (var folder in new[] { "tang", "ban", "monan", "danhmuc" })
{
    var folderPath = Path.Combine(imagesPath, folder);
    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();


app.UseCors("AllowVue");

app.UseAuthentication();
app.UseMiddleware<SecurityStampMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub"); // Ánh xạ đường truyền Hub

app.Run();