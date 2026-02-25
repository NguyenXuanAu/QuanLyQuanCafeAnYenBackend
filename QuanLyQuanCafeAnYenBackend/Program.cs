using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuanLyQuanCafeAnYenBackend.Controllers;
using QuanLyQuanCafeAnYenBackend.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Cấu hình bỏ qua lỗi vòng lặp JSON vô tận của Entity Framework
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Kết nối Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyQuanCafeDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. JWT Settings
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

// 3. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVue", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Tạo thư mục wwwroot/images nếu chưa có
var webRootPath = app.Environment.WebRootPath;

if (string.IsNullOrEmpty(webRootPath))
{
    var currentDir = Directory.GetCurrentDirectory();
    webRootPath = currentDir.Contains("bin")
        ? Path.Combine(Directory.GetParent(currentDir)?.Parent?.Parent?.FullName ?? currentDir, "wwwroot")
        : Path.Combine(currentDir, "wwwroot");

    app.Environment.WebRootPath = webRootPath;
}

if (!Directory.Exists(webRootPath))
    Directory.CreateDirectory(webRootPath);

var imagesPath = Path.Combine(webRootPath, "images");
if (!Directory.Exists(imagesPath))
    Directory.CreateDirectory(imagesPath);

foreach (var folder in new[] { "tang", "ban", "monan", "danhmuc", "feedback" })
{
    var folderPath = Path.Combine(imagesPath, folder);
    if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);
}

// 5. Middleware pipeline

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

app.Run();