using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class ChiTietDonHang
{
    public string MaChiTiet { get; set; } = null!;

    public int SoLuong { get; set; }

    public string? Size { get; set; }

    public string? MucDo { get; set; }

    public decimal GiaTaiThoiDiem { get; set; }

    public string? GhiChu { get; set; }

    public int? TrangThaiBep { get; set; }

    public DateTime? ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianHoanThanh { get; set; }

    public string MaDonHang { get; set; } = null!;

    public string MaMonAn { get; set; } = null!;

    public string? MaNhanVienPhaChế { get; set; }

    public virtual DonHang MaDonHangNavigation { get; set; } = null!;

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;

    public virtual NhanVien? MaNhanVienPhaChếNavigation { get; set; }
}
