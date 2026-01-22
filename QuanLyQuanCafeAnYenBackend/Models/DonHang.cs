using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class DonHang
{
    public string MaDonHang { get; set; } = null!;

    public int LoaiDonHang { get; set; }

    public DateTime? ThoiGianTao { get; set; }

    public int? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public string? MaBan { get; set; }

    public string? MaNguoiDung { get; set; }

    public string? MaDonDat { get; set; }

    public string? MaNhanVienNhan { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual Ban? MaBanNavigation { get; set; }

    public virtual DonDatBan? MaDonDatNavigation { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual NhanVien? MaNhanVienNhanNavigation { get; set; }
}
