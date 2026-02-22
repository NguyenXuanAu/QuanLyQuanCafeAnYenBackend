using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class HinhAnhDanhGium
{
    public string MaHinhAnh { get; set; } = null!;

    public string DuongDanAnh { get; set; } = null!;

    public int? ThuTuHienThi { get; set; }

    public string MaDanhGia { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual DanhGium MaDanhGiaNavigation { get; set; } = null!;
}
