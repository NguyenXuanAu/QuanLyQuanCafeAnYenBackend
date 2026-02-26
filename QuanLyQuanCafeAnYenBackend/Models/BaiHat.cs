using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class BaiHat
{
    public string MaBaiHat { get; set; } = null!;

    public string TenBaiHat { get; set; } = null!;

    public string? CaSi { get; set; }

    public string? TheLoai { get; set; }

    public int? ThoiLuong { get; set; }

    public string? LinkAudio { get; set; }

    public string? LinkLoiNhac { get; set; }

    public string? AnhBia { get; set; }

    public bool? TrangThai { get; set; }

    public string? ChuThich { get; set; }
    public string? SpotifyId { get; set; }
    public string? LinkNgheThu { get; set; }

    public virtual ICollection<ChiTietPlaylist> ChiTietPlaylists { get; set; } = new List<ChiTietPlaylist>();

    public virtual ICollection<YeuCauNhac> YeuCauNhacs { get; set; } = new List<YeuCauNhac>();
}
