using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class Playlist
{
    public string MaPlaylist { get; set; } = null!;

    public string TenPlaylist { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? AnhBia { get; set; }

    public bool? TrangThai { get; set; }

    public int? ThuTu { get; set; }

    public DateTime? ThoiGianTao { get; set; }

    public string? MaNhanVien { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<ChiTietPlaylist> ChiTietPlaylists { get; set; } = new List<ChiTietPlaylist>();

    public virtual ICollection<LichPhatNhac> LichPhatNhacs { get; set; } = new List<LichPhatNhac>();

    public virtual NhanVien? MaNhanVienNavigation { get; set; }
}
