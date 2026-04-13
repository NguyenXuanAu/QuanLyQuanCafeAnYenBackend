namespace QuanLyQuanCafeAnYenBackend.Models
{
    public class PreOrderDto
    {
        public string? MaDonDat { get; set; }
        public string? MaNguoiDung { get; set; }
        public string? GhiChu { get; set; }

        // THÊM BIẾN NÀY ĐỂ NHẬN MỨC CỌC TỪ FRONTEND (0, 30, hoặc 100)
        public int PhanTramCoc { get; set; }
        public int SoDiemMuonDung { get; set; }

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }

    public class CartItemDto
    {
        public string MaMonAn { get; set; } = null!;
        public int SoLuong { get; set; }
        public string? Size { get; set; }
        public string? MucDo { get; set; }
        public decimal GiaTaiThoiDiem { get; set; }
        public string? GhiChu { get; set; }
    }
}