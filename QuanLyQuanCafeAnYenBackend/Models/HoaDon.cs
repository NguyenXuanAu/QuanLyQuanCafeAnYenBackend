using System;
using System.Collections.Generic;

namespace QuanLyQuanCafeAnYenBackend.Models;

public partial class HoaDon
{
    public string MaHoaDon { get; set; } = null!;

    public DateTime? ThoiGianVao { get; set; }

    public DateTime? ThoiGianRa { get; set; }

    public int? ThoiGianNgoiThucTe { get; set; }

    public decimal? TongTienMonAn { get; set; }

    public decimal? TienPhuThu { get; set; }

    public decimal? TongTienThanhToan { get; set; }

    public decimal? TienGiam { get; set; }

    public string? PhuongThucThanhToan { get; set; }

    public int? TrangThai { get; set; }

    public string MaBan { get; set; } = null!;

    public string? MaDonDat { get; set; }

    public string? MaNguoiDung { get; set; }

    public string? MaNhanVienThuNgan { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();


    public virtual ICollection<LichSuDiem> LichSuDiems { get; set; } = new List<LichSuDiem>();

    public virtual Ban MaBanNavigation { get; set; } = null!;

    public virtual DonDatBan? MaDonDatNavigation { get; set; }

    public virtual NguoiDung? MaNguoiDungNavigation { get; set; }

    public virtual NhanVien? MaNhanVienThuNganNavigation { get; set; }
}
