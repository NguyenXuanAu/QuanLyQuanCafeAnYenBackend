using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class Ban
{
    public string MaBan { get; set; } = null!;

    public string TenBan { get; set; } = null!;

    public int SoGhe { get; set; }

    public string? LoaiBan { get; set; }

    public int? TrangThai { get; set; }

    public bool? CoOdien { get; set; }

    public bool? GanCuaSo { get; set; }

    public decimal? PhiDichVu { get; set; }

    public string MaTang { get; set; } = null!;

    public string? ChuThich { get; set; }

    public virtual ICollection<DonDatBan> DonDatBans { get; set; } = new List<DonDatBan>();

    public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual Tang MaTangNavigation { get; set; } = null!;

    public virtual ICollection<MuonThietBi> MuonThietBis { get; set; } = new List<MuonThietBi>();

    public virtual ICollection<YeuCauNhac> YeuCauNhacs { get; set; } = new List<YeuCauNhac>();
}
