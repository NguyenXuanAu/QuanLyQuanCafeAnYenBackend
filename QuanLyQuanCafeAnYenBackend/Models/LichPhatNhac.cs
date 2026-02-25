using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class LichPhatNhac
{
    public string MaLich { get; set; } = null!;

    public DateOnly NgayAp { get; set; }

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string MaPlaylist { get; set; } = null!;

    public string? MaTang { get; set; }

    public string? ChuThich { get; set; }

    public virtual Playlist MaPlaylistNavigation { get; set; } = null!;

    public virtual Tang? MaTangNavigation { get; set; }
}
