using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Add services
// =======================

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (BẮT BUỘC CHO VUE)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyQuanCafeDbContext>(options =>
    options.UseSqlServer(connectionString));



var app = builder.Build();

// =======================
// HTTP pipeline
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll"); // ⚠️ PHẢI đặt trước Authorization

app.UseAuthorization();

app.MapControllers();

app.Run();
