using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class NhanVien
{
    public string MaNhanVien { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string ChucVu { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public bool? TrangThai { get; set; } = true;

    public DateOnly? NgayVaoLam { get; set; }

    public decimal? LuongCoBan { get; set; }

    public string? ChuThich { get; set; }
    [JsonIgnore]
    public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    [JsonIgnore]
    public virtual ICollection<DonDatBan> DonDatBans { get; set; } = new List<DonDatBan>();
    [JsonIgnore]
    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    [JsonIgnore]
    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    [JsonIgnore]
    public virtual ICollection<MuonThietBi> MuonThietBis { get; set; } = new List<MuonThietBi>();
    [JsonIgnore]
    public virtual ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}
