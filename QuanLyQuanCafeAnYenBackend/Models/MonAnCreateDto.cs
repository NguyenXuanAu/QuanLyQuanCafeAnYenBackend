namespace QuanLyQuanCafeAnYenBackend.Models
{
    public class MonAnCreateDto
    {
        public string MaMonAn { get; set; }
        public string TenMonAn { get; set; }
        public decimal GiaTien { get; set; }
        public string DonViTinh { get; set; }
        public string MaDanhMuc { get; set; }
        public string MoTa { get; set; }
        public string ChuThich { get; set; }
        public bool ConHang { get; set; }

        public IFormFile File { get; set; }
    }
}
