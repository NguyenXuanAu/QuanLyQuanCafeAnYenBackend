using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class NguoiDung
{
    [Key]
    [Column(TypeName = "char(10)")]
    public string MaNguoiDung { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhauHash { get; set; } = null!;

    public int VaiTro { get; set; }

    public int? DiemTichLuy { get; set; }

    public int? DoTuoi { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual ICollection<DonDatBan> DonDatBans { get; set; } = new List<DonDatBan>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual ICollection<LichSuDiem> LichSuDiems { get; set; } = new List<LichSuDiem>();
}
