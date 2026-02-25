using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class ChiTietPlaylist
{
    public string MaChiTiet { get; set; } = null!;

    public int ThuTu { get; set; }

    public string MaPlaylist { get; set; } = null!;

    public string MaBaiHat { get; set; } = null!;

    public virtual BaiHat MaBaiHatNavigation { get; set; } = null!;

    public virtual Playlist MaPlaylistNavigation { get; set; } = null!;
}
