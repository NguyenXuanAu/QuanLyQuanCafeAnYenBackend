using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class NhanVien
{
    public string MaNhanVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string ChucVu { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public bool? TrangThai { get; set; }

    public DateOnly? NgayVaoLam { get; set; }

    public decimal? LuongCoBan { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

    public virtual ICollection<DonDatBan> DonDatBans { get; set; } = new List<DonDatBan>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<MuonThietBi> MuonThietBis { get; set; } = new List<MuonThietBi>();

    public virtual ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}
