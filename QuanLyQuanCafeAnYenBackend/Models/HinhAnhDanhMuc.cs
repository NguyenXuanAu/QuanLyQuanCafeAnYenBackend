using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class HinhAnhDanhMuc
{
    public string MaHinhAnh { get; set; } = null!;

    public string DuongDanAnh { get; set; } = null!;

    public bool? LaAnhDaiDien { get; set; }

    public int? ThuTuHienThi { get; set; }

    public string MaDanhMuc { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual DanhMuc MaDanhMucNavigation { get; set; } = null!;
}
