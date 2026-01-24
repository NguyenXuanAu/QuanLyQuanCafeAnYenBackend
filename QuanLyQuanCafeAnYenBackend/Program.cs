using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyQuanCafeDbContext>(options =>
    options.UseSqlServer(connectionString));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

var webRootPath = app.Environment.WebRootPath;


if (string.IsNullOrEmpty(webRootPath))
{
    var currentDir = Directory.GetCurrentDirectory();

    if (currentDir.Contains("bin"))
    {
        var projectRoot = Directory.GetParent(currentDir)?.Parent?.Parent?.FullName;

        if (projectRoot != null && Directory.Exists(projectRoot))
        {
            webRootPath = Path.Combine(projectRoot, "wwwroot");
        }
        else
        {
            webRootPath = Path.Combine(currentDir, "wwwroot");
        }
    }
    else
    {
        webRootPath = Path.Combine(currentDir, "wwwroot");
    }
    app.Environment.WebRootPath = webRootPath;
}

if (!Directory.Exists(webRootPath))
{
    Directory.CreateDirectory(webRootPath);
}
var imagesPath = Path.Combine(webRootPath, "images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}
var subFolders = new[] { "tang", "ban", "monan", "danhmuc" };
foreach (var folder in subFolders)
{
    var folderPath = Path.Combine(imagesPath, folder);
    if (!Directory.Exists(folderPath))
    {
        Directory.CreateDirectory(folderPath);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CRITICAL: UseStaticFiles PHẢI đặt trước UseCors và UseAuthorization
app.UseStaticFiles(); // Cho phép truy cập file tĩnh trong wwwroot

app.UseCors("AllowVueApp");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


app.Run();