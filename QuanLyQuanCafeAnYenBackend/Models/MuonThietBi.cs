using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class MuonThietBi
{
    public string MaLuotMuon { get; set; } = null!;

    public string TenThietBi { get; set; } = null!;

    public DateTime? ThoiGianMuon { get; set; }

    public DateTime? ThoiGianTra { get; set; }

    public int? TrangThai { get; set; }

    public string MaBan { get; set; } = null!;

    public string? MaNhanVienChoMuon { get; set; }

    public virtual Ban? MaBanNavigation { get; set; }

    public virtual NhanVien? MaNhanVienChoMuonNavigation { get; set; }
    [Required]
    public required string MaNguoiDung { get; set; }
}
