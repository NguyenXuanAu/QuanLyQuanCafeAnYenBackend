using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class DanhMuc
{
    public string MaDanhMuc { get; set; } = null!;

    public string TenDanhMuc { get; set; } = null!;

    public int? ThuTuHienThi { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<HinhAnhDanhMuc> HinhAnhDanhMucs { get; set; } = new List<HinhAnhDanhMuc>();

    public virtual ICollection<MonAn> MonAns { get; set; } = new List<MonAn>();
}
