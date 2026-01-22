using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class LichSuDiem
{
    public string MaGiaoDich { get; set; } = null!;

    public int LoaiGiaoDich { get; set; }

    public int SoDiem { get; set; }

    public string? MoTa { get; set; }

    public DateTime? ThoiGian { get; set; }

    public string MaNguoiDung { get; set; } = null!;

    public string? MaHoaDon { get; set; }

    public virtual HoaDon? MaHoaDonNavigation { get; set; }

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
