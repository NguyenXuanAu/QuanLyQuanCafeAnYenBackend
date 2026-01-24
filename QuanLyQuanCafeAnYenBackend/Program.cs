using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Kết nối với database SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyQuanCafeDbContext>(options => options.UseSqlServer(connectionString));
//Cấp quyền API
builder.Services.AddCors(options => {
    options.AddPolicy("AllowVueApp",
        policy => policy.AllowAnyOrigin() // Thử cho phép tất cả để test trước
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("AllowVueApp");


app.UseAuthorization();

app.MapControllers();

app.Run();
