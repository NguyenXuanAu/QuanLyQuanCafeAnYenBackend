public class MonAnFormDto
{
    public string MaMonAn { get; set; }
    public string TenMonAn { get; set; }
    public decimal GiaTien { get; set; }
    public string MaDanhMuc { get; set; }
    public string? DonViTinh { get; set; }
    public bool? ConHang { get; set; }
    public string? MoTa { get; set; }
    public IFormFile? HinhAnh { get; set; }
}
