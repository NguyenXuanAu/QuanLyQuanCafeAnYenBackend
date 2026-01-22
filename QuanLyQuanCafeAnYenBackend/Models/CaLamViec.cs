using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class CaLamViec
{
    public string MaCa { get; set; } = null!;

    public string TenCa { get; set; } = null!;

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
}
