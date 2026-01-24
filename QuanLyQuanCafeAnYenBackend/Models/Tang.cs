using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class Tang
{
    public string MaTang { get; set; } = null!;

    public string TenTang { get; set; } = null!;

    public bool? LaKhuYenTinh { get; set; }

    public string? MoTa { get; set; }

    public string? ChuThich { get; set; }

    public virtual ICollection<Ban> Bans { get; set; } = new List<Ban>();

    public virtual ICollection<HinhAnhTang> HinhAnhTangs { get; set; } = new List<HinhAnhTang>();
}
