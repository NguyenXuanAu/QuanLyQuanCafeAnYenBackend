using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class HinhAnhBan
{
    public string MaHinhAnh { get; set; } = null!;

    public string DuongDanAnh { get; set; } = null!;

    public bool? LaAnhDaiDien { get; set; }

    public int? ThuTuHienThi { get; set; }

    public string MaBan { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual Ban MaBanNavigation { get; set; } = null!;
}
