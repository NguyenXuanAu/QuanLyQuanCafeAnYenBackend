using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class DanhGium
{
    public string MaDanhGia { get; set; } = null!;

    public int SoSao { get; set; }

    public string? BinhLuan { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public string MaHoaDon { get; set; } = null!;

    public string? MaNguoiDung { get; set; }

    public virtual ICollection<HinhAnhDanhGium> HinhAnhDanhGia { get; set; } = new List<HinhAnhDanhGium>();

    public virtual HoaDon MaHoaDonNavigation { get; set; } = null!;

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }
}
