using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class YeuCauNhac
{
    public string MaYeuCau { get; set; } = null!;

    public string TenBaiHat { get; set; } = null!;

    public string? CaSi { get; set; }

    public string? LinkVideo { get; set; }

    public int? SoLuotBinhChon { get; set; }

    public bool? DaPhat { get; set; }

    public string? NguoiYeuCau { get; set; }

    public DateTime? ThoiGianYeuCau { get; set; }

    public string? MaBan { get; set; }

    public virtual Ban? MaBanNavigation { get; set; }
}
