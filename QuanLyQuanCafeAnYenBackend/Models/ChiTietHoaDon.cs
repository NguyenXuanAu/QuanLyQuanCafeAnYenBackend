using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class ChiTietHoaDon
{
    public string MaChiTiet { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal GiaTaiThoiDiemGoi { get; set; }

    public decimal ThanhTien { get; set; }

    public string? GhiChu { get; set; }

    public string MaHoaDon { get; set; } = null!;

    public string MaMonAn { get; set; } = null!;

    public virtual HoaDon MaHoaDonNavigation { get; set; } = null!;

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
