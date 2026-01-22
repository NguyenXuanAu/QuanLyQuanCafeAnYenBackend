using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class HinhAnhMonAn
{
    public string MaHinhAnh { get; set; } = null!;

    public string DuongDanAnh { get; set; } = null!;

    public bool? LaAnhDaiDien { get; set; }

    public int? ThuTuHienThi { get; set; }

    public string MaMonAn { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual MonAn MaMonAnNavigation { get; set; } = null!;
}
