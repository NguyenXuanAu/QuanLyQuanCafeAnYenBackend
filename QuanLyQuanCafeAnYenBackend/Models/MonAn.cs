using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class MonAn
{
    public string MaMonAn { get; set; } = null!;

    public string TenMonAn { get; set; } = null!;

    public decimal GiaTien { get; set; }

    public string? MoTa { get; set; }

    public bool? ConHang { get; set; }

    public string? DonViTinh { get; set; }

    public string MaDanhMuc { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<HinhAnhMonAn> HinhAnhMonAns { get; set; } = new List<HinhAnhMonAn>();

    public virtual DanhMuc MaDanhMucNavigation { get; set; } = null!;
}
