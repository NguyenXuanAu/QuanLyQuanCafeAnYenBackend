using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class PhanCong
{
    public string MaPhanCong { get; set; } = null!;

    public DateOnly NgayLam { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string MaCa { get; set; } = null!;

    public virtual CaLamViec MaCaNavigation { get; set; } = null!;

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;
}
