using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class DonDatBan
{
    public string MaDonDat { get; set; } = null!;

    public Guid? MaQr { get; set; }

    public string TenKhachHang { get; set; } = null!;

    public string SdtKhachHang { get; set; } = null!;

    public string? EmailKhachHang { get; set; }

    public DateTime ThoiGianDen { get; set; }

    public int? ThoiGianNgoiDuKien { get; set; }

    public int SoLuongNguoi { get; set; }

    public decimal? TienCoc { get; set; }

    public decimal? TongTienDatTruoc { get; set; }

    public string? GhiChu { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? ThoiGianTao { get; set; }

    public string? MaNguoiDung { get; set; }

    public string MaBan { get; set; } = null!;

    public string? MaNhanVienXuLy { get; set; }

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual Ban MaBanNavigation { get; set; } = null!;

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual NhanVien? MaNhanVienXuLyNavigation { get; set; }
}
