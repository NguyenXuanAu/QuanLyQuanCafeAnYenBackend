using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class NhanVien
{
    [Key]
    [Required(ErrorMessage = "Mã nhân viên không được để trống")]
    [StringLength(30, ErrorMessage = "Mã nhân viên không được vượt quá 30 ký tự")]
    public string MaNhanVien { get; set; } = null!;

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Họ tên không được chứa số hoặc ký tự đặc biệt")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    public string HoTen { get; set; } = null!;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [MaxLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 số")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải là chữ số và có độ dài từ 10 đến 11 ký tự")]
    public string SoDienThoai { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống")]
    [RegularExpression(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,4})+)$", ErrorMessage = "Email không đúng định dạng (VD: vidu@gmail.com)")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Chức vụ không được để trống")]
    public string ChucVu { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public bool? TrangThai { get; set; } = true;

    [DataType(DataType.Date)]
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
    [JsonIgnore]
    public virtual ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
}
