using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeAnYenBackend.Models;
using System.Text.RegularExpressions;

namespace QuanLyQuanCafeAnYenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SePayController : ControllerBase
    {
        private readonly QuanLyQuanCafeDbContext _context;

        public SePayController(QuanLyQuanCafeDbContext context)
        {
            _context = context;
        }

        [HttpPost("Webhook")]
        public async Task<IActionResult> ReceivePayment([FromBody] SePayWebhookData data)
        {
            try
            {
                Console.WriteLine($"[SePay] CÓ TIỀN VÀO: +{data.transferAmount} VND | Nội dung: {data.content}");

                if (data.transferType == "in")
                {
                    string noiDungCK = data.content.ToUpper();
                    var match = Regex.Match(noiDungCK, @"DDB\d+");

                    if (match.Success)
                    {
                        string maDonDat = match.Value;
                        var donDatBan = await _context.DonDatBans.FirstOrDefaultAsync(d => d.MaDonDat == maDonDat);

                        if (donDatBan != null)
                        {
                            // 1. Cộng dồn tiền khách đã chuyển
                            donDatBan.TienCoc = (donDatBan.TienCoc ?? 0) + data.transferAmount;

                            // 2. TÍNH TOÁN TRẠNG THÁI MỚI (1: Cọc 30%, 2: Đủ 100%)
                            int trangThaiMoi = 0;
                            decimal tongBill = donDatBan.TongTienDatTruoc ?? 0;
                            decimal tienDaThu = donDatBan.TienCoc ?? 0;

                            if (tongBill > 0)
                            {
                                // Nếu số tiền đã thu lớn hơn hoặc bằng Tổng bill (trừ hao 1000đ lỡ sai số)
                                if (tienDaThu >= (tongBill - 1000))
                                {
                                    trangThaiMoi = 2; // Thanh toán 100%
                                }
                                // Nếu chưa đủ 100% nhưng lớn hơn hoặc bằng 30%
                                else if (tienDaThu >= (tongBill * 0.3m - 1000))
                                {
                                    trangThaiMoi = 1; // Đã cọc 30%
                                }
                            }

                            // 3. ĐỒNG BỘ TRẠNG THÁI CHO CẢ 2 BẢNG (Chỉ cập nhật nếu đơn chưa bị Hủy - số 3)
                            if (donDatBan.TrangThai != 3 && trangThaiMoi > donDatBan.TrangThai)
                            {
                                // Cập nhật bảng Đơn Đặt Bàn
                                donDatBan.TrangThai = trangThaiMoi;

                                // Tìm và cập nhật luôn bảng Đơn Hàng liên kết
                                var donHang = await _context.DonHangs.FirstOrDefaultAsync(dh => dh.MaDonDat == maDonDat);
                                if (donHang != null)
                                {
                                    donHang.TrangThai = trangThaiMoi;
                                }
                            }

                            await _context.SaveChangesAsync();
                            Console.WriteLine($"[SePay] -> Đã chốt đơn {maDonDat} thành trạng thái {trangThaiMoi} thành công!");
                            return Ok(new { success = true, message = "Cập nhật thành công" });
                        }
                    }
                }

                return Ok(new { success = false, message = "Không khớp mã đơn hàng hoặc là giao dịch rút tiền." });
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SePay] Lỗi Webhook: " + ex.Message);
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class SePayWebhookData
    {
        public long id { get; set; }
        public string gateway { get; set; }
        public string transactionDate { get; set; }
        public string accountNumber { get; set; }
        public string content { get; set; }
        public decimal transferAmount { get; set; }
        public string transferType { get; set; }
    }
}