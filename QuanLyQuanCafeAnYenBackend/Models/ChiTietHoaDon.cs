using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class ChiTietHoaDon
{
    public string MaChiTiet { get; set; } = null!;

    public int SoLuong { get; set; }

    public decimal GiaTaiThoiDiemGoi { get; set; }

    public decimal ThanhTien { get; set; }

    public string? GhiChu { get; set; }

    public string MaHoaDon { get; set; }
    [ForeignKey("MaHoaDon")]
    public virtual HoaDon HoaDon { get; set; }

    public string MaMonAn { get; set; }
    [ForeignKey("MaMonAn")]
    public virtual MonAn MonAn { get; set; }
}
